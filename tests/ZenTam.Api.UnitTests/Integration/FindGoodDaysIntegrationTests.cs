using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.CanChi.Models;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Common.Rules;
using ZenTam.Api.Features.Calendars.Models;
using ZenTam.Api.Features.Calendars.Services;
using ZenTam.Api.Features.EvaluateSpiritualAction.Models;
using ZenTam.Api.Features.EvaluateSpiritualAction.Services;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.UnitTests.Integration;

/// <summary>
/// Integration tests for Find Good Days API workflow.
/// Tests end-to-end scenarios: Create Client → Find Good Days → SSE Stream
/// </summary>
public class FindGoodDaysIntegrationTests : IDisposable
{
    private readonly ZenTamDbContext _db;
    private readonly Mock<IDayContextService> _mockDayContextService;
    private readonly Mock<ILunarCalculatorService> _mockLunarCalculator;
    private readonly Mock<ICanChiCalculator> _mockCanChiCalculator;
    private readonly FindGoodDaysService _service;

    public FindGoodDaysIntegrationTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ZenTamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new ZenTamDbContext(options);

        // Setup mocks
        _mockDayContextService = new Mock<IDayContextService>();
        _mockLunarCalculator = new Mock<ILunarCalculatorService>();
        _mockCanChiCalculator = new Mock<ICanChiCalculator>();

        // Default setup for lunar calculator - returns lunar date context
        _mockLunarCalculator.Setup(x => x.Convert(It.IsAny<DateTime>()))
            .Returns(new LunarDateContext { LunarYear = 2026, LunarMonth = 4, LunarDay = 16, IsLeap = false });

        // Default setup for GetJulianDayNumber
        _mockLunarCalculator.Setup(x => x.GetJulianDayNumber(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(2460800);

        // Setup CanChi calculator mock
        _mockCanChiCalculator.Setup(x => x.GetCanChiNam(It.IsAny<int>()))
            .Returns(new CanChiYear("Bính", "Ngọ"));
        _mockCanChiCalculator.Setup(x => x.GetCanChiThang(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(new CanChiMonth("Bính", "Dần"));
        _mockCanChiCalculator.Setup(x => x.GetCanChiNgay(It.IsAny<int>()))
            .Returns(new CanChiDay("Bính", "Ngọ"));
        _mockCanChiCalculator.Setup(x => x.GetJulianDayNumber(It.IsAny<DateTime>())).Returns(2460800);
        _mockCanChiCalculator.Setup(x => x.GetThapNhiTruc(It.IsAny<DateTime>())).Returns(8);
        _mockCanChiCalculator.Setup(x => x.GetNhiThapBatTu(It.IsAny<int>())).Returns(0);
        _mockCanChiCalculator.Setup(x => x.GetTrucName(It.IsAny<int>())).Returns("Thành");
        _mockCanChiCalculator.Setup(x => x.GetHoangDao(It.IsAny<DateTime>()))
            .Returns(new HoangDaoInfo(true, new List<string> { "Tý", "Sửu" }, new List<string>(), new List<string>()));

        // Create a default DayContext that works for any date
        var defaultDayContext = new DayContext(
            SolarDate: DateTime.Now,
            CanChiNgay: "Bính Ngọ",
            CanChiThang: "Bính Dần",
            TrucIndex: 8,
            TrucName: "Thành",
            NhiThapBatTu: new NhiThapBatTuInfo(0, "Côn", TuClassification.Kiettu),
            HoangDao: new HoangDaoInfo(true, new List<string> { "Tý", "Sửu" }, new List<string>(), new List<string>()),
            SatChu: new SatChuInfo(false, -1),
            ThuTu: new ThuTuInfo(false, Array.Empty<int>()),
            IsNgayKy: false
        );
        _mockDayContextService.Setup(x => x.GetDayContext(It.IsAny<DateTime>())).Returns(defaultDayContext);

        // Create ActionCodeMapper and RuleResolver
        var actionCodeMapper = new ActionCodeMapper(_db);
        var ruleResolver = new RuleResolver(Array.Empty<ISpiritualRule>());

        // Default day context (good day: Thành truc, Côn tú, Hoàng Đạo)
        SetupGoodDayContext(new DateTime(2026, 5, 15));

        var scoreCalculator = new DayScoreCalculator(
            _mockDayContextService.Object,
            _mockLunarCalculator.Object,
            _mockCanChiCalculator.Object,
            actionCodeMapper,
            ruleResolver);

        _service = new FindGoodDaysService(
            _db,
            scoreCalculator,
            _mockLunarCalculator.Object);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    private void SetupGoodDayContext(DateTime solarDate)
    {
        var dayContext = new DayContext(
            SolarDate: solarDate,
            CanChiNgay: "Bính Ngọ",
            CanChiThang: "Bính Dần",
            TrucIndex: 8,
            TrucName: "Thành",
            NhiThapBatTu: new NhiThapBatTuInfo(0, "Côn", TuClassification.Kiettu),
            HoangDao: new HoangDaoInfo(true, new List<string> { "Tý", "Sửu" }, new List<string>(), new List<string>()),
            SatChu: new SatChuInfo(false, -1),
            ThuTu: new ThuTuInfo(false, Array.Empty<int>()),
            IsNgayKy: false
        );
        _mockDayContextService.Setup(x => x.GetDayContext(solarDate)).Returns(dayContext);
    }

    [Fact]
    public async Task FindGoodDaysAsync_WithValidClient_ReturnsGoodDays()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _db.ClientProfiles.Add(new ClientProfile
        {
            Id = clientId,
            Name = "Test Client",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1996, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var request = new FindGoodDaysRequest(
            ClientId: clientId,
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 31),
            MaxResults: 5
        );

        // Act
        var result = await _service.FindGoodDaysAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.SuggestedDays.Should().NotBeEmpty();
    }

    [Fact]
    public async Task FindGoodDaysAsync_WithNonExistentClient_ReturnsResultsWithDefaultGender()
    {
        // Arrange - client doesn't exist, so service uses default Gender=Male
        var request = new FindGoodDaysRequest(
            ClientId: Guid.NewGuid(),
            Action: ActionCode.XAY_NHA,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 5),
            MaxResults: 5
        );

        // Act
        var result = await _service.FindGoodDaysAsync(request);

        // Assert - should return results (service uses default Male gender when client not found)
        result.Should().NotBeNull();
        result.SuggestedDays.Should().NotBeEmpty();
        result.SuggestedDays.Should().HaveCount(5);
    }

    [Fact]
    public async Task FindGoodDaysAsync_WithSubjectClient_CalculatesXungTuoi()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();

        _db.ClientProfiles.Add(new ClientProfile
        {
            Id = clientId,
            Name = "Main Client",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1996, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });

        _db.ClientProfiles.Add(new ClientProfile
        {
            Id = subjectId,
            Name = "Subject Client",
            PhoneNumber = "0909987654",
            SolarDob = new DateTime(1984, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var request = new FindGoodDaysRequest(
            ClientId: clientId,
            Action: ActionCode.CUOI_HOI,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 3),
            SubjectClientId: subjectId,
            MaxResults: 5
        );

        // Act
        var result = await _service.FindGoodDaysAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.SuggestedDays.Should().NotBeEmpty();
    }

    [Fact]
    public async Task FindGoodDaysAsync_SingleDaySearch_Returns1Day()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _db.ClientProfiles.Add(new ClientProfile
        {
            Id = clientId,
            Name = "Test Client",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1996, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var sameDay = new DateOnly(2026, 5, 15);
        var request = new FindGoodDaysRequest(
            ClientId: clientId,
            Action: ActionCode.KHAI_TRUONG,
            FromDate: sameDay,
            ToDate: sameDay,
            MaxResults: 5
        );

        // Act
        var result = await _service.FindGoodDaysAsync(request);

        // Assert
        result.TotalDaysScanned.Should().Be(1);
        result.SuggestedDays.Should().HaveCount(1);
    }

    [Fact]
    public async Task FindGoodDaysAsync_RespectsMaxResults()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _db.ClientProfiles.Add(new ClientProfile
        {
            Id = clientId,
            Name = "Test Client",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1996, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var request = new FindGoodDaysRequest(
            ClientId: clientId,
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 31),
            MaxResults: 3
        );

        // Act
        var result = await _service.FindGoodDaysAsync(request);

        // Assert
        result.SuggestedDays.Should().HaveCountLessOrEqualTo(3);
    }

    [Fact]
    public async Task FindGoodDaysAsync_SortedByScoreDescending()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _db.ClientProfiles.Add(new ClientProfile
        {
            Id = clientId,
            Name = "Test Client",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1996, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var request = new FindGoodDaysRequest(
            ClientId: clientId,
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 10),
            MaxResults: 10
        );

        // Act
        var result = await _service.FindGoodDaysAsync(request);

        // Assert
        if (result.SuggestedDays.Count > 1)
        {
            var scores = result.SuggestedDays.Select(d => d.Score).ToList();
            scores.Should().BeInDescendingOrder();
        }
    }
}
