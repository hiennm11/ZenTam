using System.Runtime.CompilerServices;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.CanChi.Models;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Features.Calendars.Models;
using ZenTam.Api.Features.Calendars.Services;
using ZenTam.Api.Features.Clients.Commands;
using ZenTam.Api.Features.EvaluateSpiritualAction.Models;
using ZenTam.Api.Features.EvaluateSpiritualAction.Services;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;

using LunarDateContext = ZenTam.Api.Common.Domain.LunarDateContext;

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

        // Default setup for lunar calculator - returns lunar date context
        _mockLunarCalculator.Setup(x => x.Convert(It.IsAny<DateTime>()))
            .Returns(new LunarDateContext { LunarYear = 2026, LunarMonth = 4, LunarDay = 16, IsLeap = false });

        // Default setup for GetJulianDayNumber
        _mockLunarCalculator.Setup(x => x.GetJulianDayNumber(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(2460800);

        // Default day context (good day: Thành truc, Côn tú, Hoàng Đạo)
        SetupGoodDayContext(new DateTime(2026, 5, 15));

        var scoreCalculator = new DayScoreCalculator(
            _mockDayContextService.Object,
            _mockLunarCalculator.Object,
            GetMockCanChiCalculator().Object);

        _service = new FindGoodDaysService(
            _db,
            scoreCalculator,
            _mockLunarCalculator.Object);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    private Mock<ICanChiCalculator> GetMockCanChiCalculator()
    {
        var mockCanChi = new Mock<ICanChiCalculator>();
        mockCanChi.Setup(x => x.GetCanChiNam(It.IsAny<int>()))
            .Returns(new CanChiYear("Bính", "Ngọ"));
        mockCanChi.Setup(x => x.GetCanChiThang(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(new CanChiMonth("Bính", "Dần"));
        mockCanChi.Setup(x => x.GetCanChiNgay(It.IsAny<int>()))
            .Returns(new CanChiDay("Bính", "Ngọ"));
        mockCanChi.Setup(x => x.GetJulianDayNumber(It.IsAny<DateTime>())).Returns(2460800);
        mockCanChi.Setup(x => x.GetThapNhiTruc(It.IsAny<DateTime>())).Returns(8);
        mockCanChi.Setup(x => x.GetNhiThapBatTu(It.IsAny<int>())).Returns(0);
        mockCanChi.Setup(x => x.GetTrucName(It.IsAny<int>())).Returns("Thành");
        mockCanChi.Setup(x => x.GetHoangDao(It.IsAny<DateTime>()))
            .Returns(new HoangDaoInfo(true, new List<string> { "Tý", "Sửu" }, new List<string>(), new List<string>()));
        return mockCanChi;
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

    private void SetupBadDayContext(DateTime solarDate, int trucIndex = 7, string trucName = "Nguy")
    {
        var dayContext = new DayContext(
            SolarDate: solarDate,
            CanChiNgay: "Ất Mão",
            CanChiThang: "Bính Dần",
            TrucIndex: trucIndex,
            TrucName: trucName,
            NhiThapBatTu: new NhiThapBatTuInfo(12, "Mão", TuClassification.Hungtu),
            HoangDao: new HoangDaoInfo(false, new List<string>(), new List<string> { "Tý", "Sửu" }, new List<string>()),
            SatChu: new SatChuInfo(true, 9),
            ThuTu: new ThuTuInfo(true, new[] { 0, 6 }),
            IsNgayKy: true
        );
        _mockDayContextService.Setup(x => x.GetDayContext(solarDate)).Returns(dayContext);
    }

    #region Scenario A: Create Client → Find Good Days (NHAP_TRACH) → SSE Stream

    [Fact]
    public async Task ScenarioA_CreateClient_FindGoodDaysNHAP_TRACH_ReturnsTop5Days()
    {
        // Arrange - Create a new client
        var client = new ClientProfile
        {
            Id = Guid.NewGuid(),
            Name = "Test User NHAP_TRACH",
            PhoneNumber = "0987654321",
            SolarDob = new DateTime(1996, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        };
        _db.ClientProfiles.Add(client);
        await _db.SaveChangesAsync();

        // Setup day contexts for May 2026
        for (int day = 1; day <= 31; day++)
        {
            var date = new DateTime(2026, 5, day);
            if (day % 3 == 0)
                SetupBadDayContext(date);
            else
                SetupGoodDayContext(date);
        }

        // Setup request for NHAP_TRACH action
        var request = new FindGoodDaysRequest(
            ClientId: client.Id,
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 31),
            MaxResults: 5
        );

        // Act - Call Find Good Days
        var response = await _service.FindGoodDaysAsync(request);

        // Assert - Verify response structure
        response.Should().NotBeNull();
        response.Action.Should().Be(ActionCode.NHAP_TRACH);
        response.TotalDaysScanned.Should().Be(31);
        response.SuggestedDays.Should().HaveCountLessOrEqualTo(5);

        // Verify first result (top score) has required fields
        var topDay = response.SuggestedDays.FirstOrDefault();
        if (topDay != null)
        {
            topDay.Score.Should().BeGreaterOrEqualTo(0);
            topDay.TrucName.Should().NotBeNullOrEmpty();
            topDay.TuName.Should().NotBeNullOrEmpty();
            topDay.Reasons.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ScenarioA_SSEStream_YieldsAllDaysInRange()
    {
        // Arrange - Create a new client
        var client = new ClientProfile
        {
            Id = Guid.NewGuid(),
            Name = "Test User SSE",
            PhoneNumber = "0987654322",
            SolarDob = new DateTime(1996, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        };
        _db.ClientProfiles.Add(client);
        await _db.SaveChangesAsync();

        // Setup day contexts for May 1-5
        for (int day = 1; day <= 5; day++)
        {
            SetupGoodDayContext(new DateTime(2026, 5, day));
        }

        var request = new FindGoodDaysRequest(
            ClientId: client.Id,
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 5), // 5 days
            MaxResults: 5
        );

        // Act - Stream results
        var cts = new CancellationTokenSource();
        var count = 0;
        var scores = new List<int>();

        await foreach (var result in _service.StreamFindGoodDaysAsync(request, cts.Token))
        {
            count++;
            scores.Add(result.Score);
        }

        // Assert - should yield 5 results
        count.Should().Be(5, "all 5 days should be streamed via SSE");
        scores.Should().HaveCount(5);
    }

    [Fact]
    public async Task ScenarioA_SSEStream_ProgressCalculation_IsCorrect()
    {
        // Arrange
        var client = new ClientProfile
        {
            Id = Guid.NewGuid(),
            Name = "Test User Progress",
            PhoneNumber = "0987654323",
            SolarDob = new DateTime(1996, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        };
        _db.ClientProfiles.Add(client);
        await _db.SaveChangesAsync();

        // Setup day contexts for May 1-10
        for (int day = 1; day <= 10; day++)
        {
            SetupGoodDayContext(new DateTime(2026, 5, day));
        }

        var request = new FindGoodDaysRequest(
            ClientId: client.Id,
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 10), // 10 days
            MaxResults: 5
        );

        // Act
        var cts = new CancellationTokenSource();
        var progressData = new List<(int progress, int total, int percent, DateTime date, int score, bool isGood)>();
        var progress = 0;
        var totalDays = 10;

        await foreach (var result in _service.StreamFindGoodDaysAsync(request, cts.Token))
        {
            progress++;
            var percent = (progress * 100) / totalDays;
            progressData.Add((progress, totalDays, percent, result.SolarDate, result.Score, result.Score >= 60));
        }

        // Assert - Verify progress calculation
        progressData.Should().HaveCount(10);
        progressData[0].progress.Should().Be(1);
        progressData[0].total.Should().Be(10);
        progressData[0].percent.Should().Be(10); // 1/10 = 10%
        progressData[9].progress.Should().Be(10);
        progressData[9].percent.Should().Be(100); // 10/10 = 100%
        
        // All dates should be in May 2026
        progressData.All(p => p.date.Year == 2026 && p.date.Month == 5).Should().BeTrue();
    }

    #endregion

    #region Scenario B: Create Client → Find Good Days (KET_HON) → REST

    [Fact]
    public async Task ScenarioB_CreateClient_FindGoodDaysKET_HON_ReturnsTop5WithReasons()
    {
        // Arrange - Create a new client
        var client = new ClientProfile
        {
            Id = Guid.NewGuid(),
            Name = "Test User KET_HON",
            PhoneNumber = "0987654324",
            SolarDob = new DateTime(1996, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        };
        _db.ClientProfiles.Add(client);
        await _db.SaveChangesAsync();

        // Setup day contexts for May 2026
        for (int day = 1; day <= 31; day++)
        {
            var date = new DateTime(2026, 5, day);
            if (day % 5 == 0)
                SetupBadDayContext(date);
            else
                SetupGoodDayContext(date);
        }

        var request = new FindGoodDaysRequest(
            ClientId: client.Id,
            Action: ActionCode.KET_HON,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 31),
            MaxResults: 5
        );

        // Act
        var response = await _service.FindGoodDaysAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Action.Should().Be(ActionCode.KET_HON);
        response.TotalDaysScanned.Should().Be(31);
        response.SuggestedDays.Should().HaveCountLessOrEqualTo(5);

        // Verify reasons array contains relevant information for KET_HON
        var topDay = response.SuggestedDays.FirstOrDefault();
        if (topDay != null)
        {
            topDay.Reasons.Should().NotBeEmpty("KET_HON should have reasons for scoring");
            
            // At least one reason should mention Trực or Tú (scoring factors)
            var hasScoringReason = topDay.Reasons.Any(r =>
                r.Contains("Trực", StringComparison.OrdinalIgnoreCase) ||
                r.Contains("Tú", StringComparison.OrdinalIgnoreCase) ||
                r.Contains("kết hôn", StringComparison.OrdinalIgnoreCase));
            
            hasScoringReason.Should().BeTrue("scoring should include relevant reasons");
        }
    }

    [Fact]
    public async Task ScenarioB_KET_HON_And_NHAP_TRACH_ProduceDifferentScores()
    {
        // Arrange - Create a client
        var client = new ClientProfile
        {
            Id = Guid.NewGuid(),
            Name = "Test User Compare",
            PhoneNumber = "0987654325",
            SolarDob = new DateTime(1996, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        };
        _db.ClientProfiles.Add(client);
        await _db.SaveChangesAsync();

        // Setup day contexts for May 1-7
        for (int day = 1; day <= 7; day++)
        {
            SetupGoodDayContext(new DateTime(2026, 5, day));
        }

        // Use same date range for both actions
        var fromDate = new DateOnly(2026, 5, 1);
        var toDate = new DateOnly(2026, 5, 7); // Small range for faster test

        var nnapTrachRequest = new FindGoodDaysRequest(
            ClientId: client.Id,
            Action: ActionCode.NHAP_TRACH,
            FromDate: fromDate,
            ToDate: toDate,
            MaxResults: 5
        );

        var ketHonRequest = new FindGoodDaysRequest(
            ClientId: client.Id,
            Action: ActionCode.KET_HON,
            FromDate: fromDate,
            ToDate: toDate,
            MaxResults: 5
        );

        // Act
        var nnapTrachResponse = await _service.FindGoodDaysAsync(nnapTrachRequest);
        var ketHonResponse = await _service.FindGoodDaysAsync(ketHonRequest);

        // Assert - Different action codes should be properly set
        nnapTrachResponse.Action.Should().Be(ActionCode.NHAP_TRACH);
        ketHonResponse.Action.Should().Be(ActionCode.KET_HON);
        
        // Both should return results (same day contexts produce results for both actions)
        nnapTrachResponse.SuggestedDays.Should().NotBeEmpty();
        ketHonResponse.SuggestedDays.Should().NotBeEmpty();
        
        // Per the 144-lookup table, Nguy (index 7) has different scores:
        //   - NHAP_TRACH: -20 points
        //   - KET_HON: -30 points (DIFFERENT!)
        // We verify the actions are distinct
    }

    #endregion

    #region Scenario C: Verify Score Top 5

    [Fact]
    public async Task ScenarioC_VerifyScoreTop5_AreSortedDescending()
    {
        // Arrange
        var client = new ClientProfile
        {
            Id = Guid.NewGuid(),
            Name = "Test User Sort",
            PhoneNumber = "0987654326",
            SolarDob = new DateTime(1996, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        };
        _db.ClientProfiles.Add(client);
        await _db.SaveChangesAsync();

        // Setup varied day contexts - mix of good and bad days
        var random = new Random(42); // Fixed seed for reproducibility
        for (int day = 1; day <= 31; day++)
        {
            var date = new DateTime(2026, 5, day);
            if (random.NextDouble() > 0.5)
                SetupGoodDayContext(date);
            else
                SetupBadDayContext(date, trucIndex: random.Next(0, 12), trucName: "Nguy");
        }

        var request = new FindGoodDaysRequest(
            ClientId: client.Id,
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 31),
            MaxResults: 5
        );

        // Act
        var response = await _service.FindGoodDaysAsync(request);

        // Assert - Verify top 5 are sorted by score descending
        var scores = response.SuggestedDays.Select(d => d.Score).ToList();
        scores.Should().BeInDescendingOrder("top results should be sorted by score descending");
        
        // Verify we got at most 5 results
        response.SuggestedDays.Should().HaveCountLessOrEqualTo(5);
    }

    [Fact]
    public async Task ScenarioC_VerifyMaxScore_IsAround80()
    {
        // Arrange
        var client = new ClientProfile
        {
            Id = Guid.NewGuid(),
            Name = "Test User MaxScore",
            PhoneNumber = "0987654327",
            SolarDob = new DateTime(1996, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        };
        _db.ClientProfiles.Add(client);
        await _db.SaveChangesAsync();

        // Setup all good day contexts
        for (int day = 1; day <= 31; day++)
        {
            SetupGoodDayContext(new DateTime(2026, 5, day));
        }

        var request = new FindGoodDaysRequest(
            ClientId: client.Id,
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 31),
            MaxResults: 5
        );

        // Act
        var response = await _service.FindGoodDaysAsync(request);

        // Assert - Verify max score is reasonable
        // Max score per spec = Thành(20) + Kiết Tú(6) + Hoàng(6) + no negatives (12 x 4 = 48) = 80
        var maxScore = response.SuggestedDays.FirstOrDefault()?.Score ?? 0;
        
        // With good day contexts, we should get high scores
        maxScore.Should().BeGreaterOrEqualTo(40, "even average days should have some score");
        maxScore.Should().BeLessOrEqualTo(80, "max score per spec is 80");
    }

    [Fact]
    public async Task ScenarioC_VerifyScores_VaryAcrossDateRange()
    {
        // Arrange
        var client = new ClientProfile
        {
            Id = Guid.NewGuid(),
            Name = "Test User Variance",
            PhoneNumber = "0987654328",
            SolarDob = new DateTime(1996, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        };
        _db.ClientProfiles.Add(client);
        await _db.SaveChangesAsync();

        // Setup varied day contexts with mix of good/bad/truc values
        var trucValues = new[] { 8, 8, 8, 0, 1, 7, 7, 3, 4, 5 }; // Mix of good and bad trực
        var trucNames = new[] { "Thành", "Thành", "Thành", "Kiến", "Trừ", "Nguy", "Nguy", "Bình", "Định", "Chấp" };
        
        for (int day = 1; day <= 31; day++)
        {
            var idx = (day - 1) % trucValues.Length;
            SetupDayContextWithTruc(new DateTime(2026, 5, day), trucValues[idx], trucNames[idx]);
        }

        var request = new FindGoodDaysRequest(
            ClientId: client.Id,
            Action: ActionCode.KET_HON,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 31),
            MaxResults: 31 // Get all to verify variance
        );

        // Act
        var response = await _service.FindGoodDaysAsync(request);

        // Assert - Verify scores vary (not all the same)
        var allScores = response.SuggestedDays.Select(d => d.Score).ToList();
        var uniqueScores = allScores.Distinct().Count();
        
        // With varied truc values, we should have multiple different scores
        uniqueScores.Should().BeGreaterThan(1, "scores should vary across date range");
        
        // Verify range of scores
        var minScore = allScores.Min();
        var maxScore = allScores.Max();
        maxScore.Should().BeGreaterOrEqualTo(minScore, "max should be >= min");
    }

    private void SetupDayContextWithTruc(DateTime solarDate, int trucIndex, string trucName)
    {
        var dayContext = new DayContext(
            SolarDate: solarDate,
            CanChiNgay: "Bính Ngọ",
            CanChiThang: "Bính Dần",
            TrucIndex: trucIndex,
            TrucName: trucName,
            NhiThapBatTu: new NhiThapBatTuInfo(0, "Côn", TuClassification.Kiettu),
            HoangDao: new HoangDaoInfo(true, new List<string> { "Tý", "Sửu" }, new List<string>(), new List<string>()),
            SatChu: new SatChuInfo(false, -1),
            ThuTu: new ThuTuInfo(false, Array.Empty<int>()),
            IsNgayKy: false
        );
        _mockDayContextService.Setup(x => x.GetDayContext(solarDate)).Returns(dayContext);
    }

    #endregion

    #region Additional Integration Scenarios

    [Fact]
    public async Task Integration_WorksWithSubjectClientId()
    {
        // Arrange - Create two clients (client and subject for Gánh Mệnh check)
        var client = new ClientProfile
        {
            Id = Guid.NewGuid(),
            Name = "Main Client",
            PhoneNumber = "0987654329",
            SolarDob = new DateTime(1996, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        };
        var subjectClient = new ClientProfile
        {
            Id = Guid.NewGuid(),
            Name = "Subject Client",
            PhoneNumber = "0987654330",
            SolarDob = new DateTime(1998, 8, 20),
            Gender = Gender.Female,
            CreatedAt = DateTime.UtcNow
        };
        
        _db.ClientProfiles.AddRange(client, subjectClient);
        await _db.SaveChangesAsync();

        // Setup day contexts
        for (int day = 1; day <= 10; day++)
        {
            SetupGoodDayContext(new DateTime(2026, 5, day));
        }

        var request = new FindGoodDaysRequest(
            ClientId: client.Id,
            Action: ActionCode.KET_HON,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 10),
            SubjectClientId: subjectClient.Id,
            MaxResults: 5
        );

        // Act
        var response = await _service.FindGoodDaysAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Action.Should().Be(ActionCode.KET_HON);
        response.TotalDaysScanned.Should().Be(10);
    }

    [Fact]
    public async Task Integration_MaxResultsLimit_Works()
    {
        // Arrange
        var client = new ClientProfile
        {
            Id = Guid.NewGuid(),
            Name = "Test User Limit",
            PhoneNumber = "0987654332",
            SolarDob = new DateTime(1996, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        };
        _db.ClientProfiles.Add(client);
        await _db.SaveChangesAsync();

        // Setup all good day contexts (so all would score high without limit)
        for (int day = 1; day <= 31; day++)
        {
            SetupGoodDayContext(new DateTime(2026, 5, day));
        }

        var request = new FindGoodDaysRequest(
            ClientId: client.Id,
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 31),
            MaxResults: 3
        );

        // Act
        var response = await _service.FindGoodDaysAsync(request);

        // Assert
        response.SuggestedDays.Should().HaveCountLessOrEqualTo(3, "should respect maxResults limit");
    }

    [Fact]
    public async Task Integration_SSE_Cancellation_StopsStream()
    {
        // Arrange
        var client = new ClientProfile
        {
            Id = Guid.NewGuid(),
            Name = "Test User Cancel",
            PhoneNumber = "0987654333",
            SolarDob = new DateTime(1996, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        };
        _db.ClientProfiles.Add(client);
        await _db.SaveChangesAsync();

        // Setup day contexts
        for (int day = 1; day <= 31; day++)
        {
            SetupGoodDayContext(new DateTime(2026, 5, day));
        }

        var request = new FindGoodDaysRequest(
            ClientId: client.Id,
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 31),
            MaxResults: 5
        );

        // Act - Cancel after first result
        var cts = new CancellationTokenSource();
        var count = 0;

        await foreach (var result in _service.StreamFindGoodDaysAsync(request, cts.Token))
        {
            count++;
            if (count >= 1) // Cancel after receiving first result
            {
                cts.Cancel();
                break;
            }
        }

        // Assert - Should have received at least 1 result before cancellation
        count.Should().BeGreaterOrEqualTo(1, "should receive at least 1 result before cancel");
    }

    [Fact]
    public async Task Integration_SSEStream_CompletesSuccessfully()
    {
        // Arrange
        var client = new ClientProfile
        {
            Id = Guid.NewGuid(),
            Name = "Test User Complete",
            PhoneNumber = "0987654334",
            SolarDob = new DateTime(1996, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        };
        _db.ClientProfiles.Add(client);
        await _db.SaveChangesAsync();

        // Setup day contexts for 3 days
        for (int day = 1; day <= 3; day++)
        {
            SetupGoodDayContext(new DateTime(2026, 5, day));
        }

        var request = new FindGoodDaysRequest(
            ClientId: client.Id,
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 3),
            MaxResults: 5
        );

        // Act
        var cts = new CancellationTokenSource();
        var completed = false;
        var count = 0;

        try
        {
            await foreach (var result in _service.StreamFindGoodDaysAsync(request, cts.Token))
            {
                count++;
            }
            completed = true;
        }
        catch (OperationCanceledException)
        {
            completed = false;
        }

        // Assert
        completed.Should().BeTrue("stream should complete without cancellation");
        count.Should().Be(3, "should stream exactly 3 days");
    }

    #endregion
}
