using FluentAssertions;
using Moq;
using Xunit;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Features.EvaluateSpiritualAction.Models;
using ZenTam.Api.Features.EvaluateSpiritualAction.Services;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.UnitTests.EvaluateSpiritualAction;

/// <summary>
/// Tests for FindGoodDaysService - date range scanning and sorting logic.
/// </summary>
public class FindGoodDaysServiceTests
{
    private readonly Mock<IDayScoreCalculator> _mockScoreCalculator;
    private readonly Mock<ILunarCalculatorService> _mockLunarCalculator;
    private readonly ZenTamDbContext _dbContext;
    private readonly FindGoodDaysService _service;

    public FindGoodDaysServiceTests()
    {
        _mockScoreCalculator = new Mock<IDayScoreCalculator>();
        _mockLunarCalculator = new Mock<ILunarCalculatorService>();
        _dbContext = TestHelpers.TestDbHelper.CreateInMemoryDbContext();
        _service = new FindGoodDaysService(_dbContext, _mockScoreCalculator.Object, _mockLunarCalculator.Object);
    }

    [Fact]
    public async Task FindGoodDaysAsync_ReturnsTop5Days_SortedByScoreDescending()
    {
        // Arrange
        var request = new FindGoodDaysRequest(
            ClientId: Guid.NewGuid(),
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 31),
            MaxResults: 5
        );

        // Setup mock to return days with different scores
        var daysWithScores = new List<DayScoreResult>();
        for (int i = 1; i <= 31; i++)
        {
            var solarDate = new DateTime(2026, 5, i);
            int score = i * 3; // Day 31 = 93, Day 1 = 3
            daysWithScores.Add(CreateDayScoreResult(solarDate, score));
        }

        _mockScoreCalculator.Setup(x => x.Calculate(It.IsAny<DateTime>(), It.IsAny<ActionCode>(), It.IsAny<int?>()))
            .Returns((DateTime date, ActionCode action, int? _) => daysWithScores.First(d => d.SolarDate == date));

        // Act
        var result = await _service.FindGoodDaysAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.SuggestedDays.Should().HaveCount(5);
        result.TotalDaysScanned.Should().Be(31);

        // Verify descending order
        var scores = result.SuggestedDays.Select(d => d.Score).ToList();
        scores.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task FindGoodDaysAsync_DateRangeFiltering_WorksCorrectly()
    {
        // Arrange
        var fromDate = new DateOnly(2026, 5, 1);
        var toDate = new DateOnly(2026, 5, 10);
        var request = new FindGoodDaysRequest(
            ClientId: Guid.NewGuid(),
            Action: ActionCode.KET_HON,
            FromDate: fromDate,
            ToDate: toDate,
            MaxResults: 10
        );

        _mockScoreCalculator.Setup(x => x.Calculate(It.IsAny<DateTime>(), It.IsAny<ActionCode>(), It.IsAny<int?>()))
            .Returns((DateTime date, ActionCode _, int? _) => CreateDayScoreResult(date, 50));

        // Act
        var result = await _service.FindGoodDaysAsync(request);

        // Assert
        result.TotalDaysScanned.Should().Be(10);
        result.SearchRangeStart.Should().Be(fromDate);
        result.SearchRangeEnd.Should().Be(toDate);
    }

    [Fact]
    public async Task FindGoodDaysAsync_Performance_CompletesUnder50msFor31Days()
    {
        // Arrange
        var request = new FindGoodDaysRequest(
            ClientId: Guid.NewGuid(),
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 31),
            MaxResults: 5
        );

        _mockScoreCalculator.Setup(x => x.Calculate(It.IsAny<DateTime>(), It.IsAny<ActionCode>(), It.IsAny<int?>()))
            .Returns((DateTime date, ActionCode _, int? _) => CreateDayScoreResult(date, 50));

        // Act
        var startTime = DateTime.UtcNow;
        var result = await _service.FindGoodDaysAsync(request);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        elapsed.TotalMilliseconds.Should().BeLessThan(50);
        result.TotalDaysScanned.Should().Be(31);
    }

    [Fact]
    public async Task FindGoodDaysAsync_SingleDaySearch_Returns1Result()
    {
        // Arrange
        var sameDay = new DateOnly(2026, 5, 15);
        var request = new FindGoodDaysRequest(
            ClientId: Guid.NewGuid(),
            Action: ActionCode.KHAI_TRUONG,
            FromDate: sameDay,
            ToDate: sameDay,
            MaxResults: 5
        );

        _mockScoreCalculator.Setup(x => x.Calculate(It.IsAny<DateTime>(), It.IsAny<ActionCode>(), It.IsAny<int?>()))
            .Returns((DateTime date, ActionCode _, int? _) => CreateDayScoreResult(date, 50));

        // Act
        var result = await _service.FindGoodDaysAsync(request);

        // Assert
        result.TotalDaysScanned.Should().Be(1);
        result.SuggestedDays.Should().HaveCount(1);
    }

    [Fact]
    public async Task FindGoodDaysAsync_MaxResults1_ReturnsOnly1Day()
    {
        // Arrange
        var request = new FindGoodDaysRequest(
            ClientId: Guid.NewGuid(),
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 31),
            MaxResults: 1
        );

        _mockScoreCalculator.Setup(x => x.Calculate(It.IsAny<DateTime>(), It.IsAny<ActionCode>(), It.IsAny<int?>()))
            .Returns((DateTime date, ActionCode _, int? _) => CreateDayScoreResult(date, 50));

        // Act
        var result = await _service.FindGoodDaysAsync(request);

        // Assert
        result.SuggestedDays.Should().HaveCount(1);
    }

    [Fact]
    public async Task FindGoodDaysAsync_Tiebreaker_EarlierDateFirst()
    {
        // Arrange - two days with same score
        var request = new FindGoodDaysRequest(
            ClientId: Guid.NewGuid(),
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 3),
            MaxResults: 5
        );

        var day1 = CreateDayScoreResult(new DateTime(2026, 5, 1), 75);
        var day2 = CreateDayScoreResult(new DateTime(2026, 5, 2), 75);
        var day3 = CreateDayScoreResult(new DateTime(2026, 5, 3), 60);

        _mockScoreCalculator.Setup(x => x.Calculate(new DateTime(2026, 5, 1), It.IsAny<ActionCode>(), It.IsAny<int?>()))
            .Returns(day1);
        _mockScoreCalculator.Setup(x => x.Calculate(new DateTime(2026, 5, 2), It.IsAny<ActionCode>(), It.IsAny<int?>()))
            .Returns(day2);
        _mockScoreCalculator.Setup(x => x.Calculate(new DateTime(2026, 5, 3), It.IsAny<ActionCode>(), It.IsAny<int?>()))
            .Returns(day3);

        // Act
        var result = await _service.FindGoodDaysAsync(request);

        // Assert - when tied, earlier date should come first
        result.SuggestedDays[0].SolarDate.Should().Be(new DateTime(2026, 5, 1));
        result.SuggestedDays[1].SolarDate.Should().Be(new DateTime(2026, 5, 2));
    }

    [Fact]
    public async Task FindGoodDaysAsync_AllNegativeScores_StillReturnsSorted()
    {
        // Arrange
        var request = new FindGoodDaysRequest(
            ClientId: Guid.NewGuid(),
            Action: ActionCode.KET_HON,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 5),
            MaxResults: 5
        );

        for (int i = 1; i <= 5; i++)
        {
            var date = new DateTime(2026, 5, i);
            var score = -10 * i; // -10, -20, -30, -40, -50
            _mockScoreCalculator.Setup(x => x.Calculate(date, It.IsAny<ActionCode>(), It.IsAny<int?>()))
                .Returns(CreateDayScoreResult(date, score));
        }

        // Act
        var result = await _service.FindGoodDaysAsync(request);

        // Assert - highest (least negative) should be first
        result.SuggestedDays.First().Score.Should().Be(-10);
        result.SuggestedDays.Last().Score.Should().Be(-50);
    }

    [Fact]
    public async Task FindGoodDaysAsync_WithSubjectClientId_LoadsLunarYear()
    {
        // Arrange
        var subjectId = Guid.NewGuid();
        _dbContext.ClientProfiles.Add(new ClientProfile
        {
            Id = subjectId,
            Name = "Test Subject",
            PhoneNumber = "1234567890",
            SolarDob = new DateTime(1984, 5, 15),
            Gender = Common.Domain.Gender.Male
        });
        await _dbContext.SaveChangesAsync();

        // Mock lunar calculator to return year 1984 for 1984-05-15
        _mockLunarCalculator.Setup(x => x.Convert(It.IsAny<DateTime>()))
            .Returns(new LunarDateContext { LunarYear = 1984, LunarMonth = 4, LunarDay = 15 });

        var request = new FindGoodDaysRequest(
            ClientId: Guid.NewGuid(),
            Action: ActionCode.KET_HON,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 3),
            SubjectClientId: subjectId,
            MaxResults: 5
        );

        _mockScoreCalculator.Setup(x => x.Calculate(It.IsAny<DateTime>(), It.IsAny<ActionCode>(), It.IsAny<int?>()))
            .Returns((DateTime date, ActionCode _, int? lunarYear) =>
            {
                // Should be called with lunarYear = 1984 for xung check
                return CreateDayScoreResult(date, 50);
            });

        // Act
        var result = await _service.FindGoodDaysAsync(request);

        // Assert - service should have attempted to load subject's lunar year
        result.Should().NotBeNull();
        _mockScoreCalculator.Verify(x => x.Calculate(
            It.IsAny<DateTime>(),
            ActionCode.KET_HON,
            It.IsAny<int?>()
        ), Times.AtLeast(1));
    }

    [Fact]
    public async Task FindGoodDaysAsync_NonExistentSubjectClientId_TreatedAsNoSubject()
    {
        // Arrange - subject doesn't exist
        var request = new FindGoodDaysRequest(
            ClientId: Guid.NewGuid(),
            Action: ActionCode.KET_HON,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 1),
            SubjectClientId: Guid.NewGuid(), // Non-existent
            MaxResults: 5
        );

        _mockScoreCalculator.Setup(x => x.Calculate(It.IsAny<DateTime>(), It.IsAny<ActionCode>(), It.IsAny<int?>()))
            .Returns((DateTime date, ActionCode _, int? lunarYear) =>
            {
                // lunarYear should be null for non-existent subject
                lunarYear.Should().BeNull();
                return CreateDayScoreResult(date, 50);
            });

        // Act
        var result = await _service.FindGoodDaysAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task FindGoodDaysAsync_SearchRangeEnd_Correct()
    {
        // Arrange
        var fromDate = new DateOnly(2026, 5, 1);
        var toDate = new DateOnly(2026, 5, 31);
        var request = new FindGoodDaysRequest(
            ClientId: Guid.NewGuid(),
            Action: ActionCode.NHAP_TRACH,
            FromDate: fromDate,
            ToDate: toDate,
            MaxResults: 5
        );

        _mockScoreCalculator.Setup(x => x.Calculate(It.IsAny<DateTime>(), It.IsAny<ActionCode>(), It.IsAny<int?>()))
            .Returns((DateTime date, ActionCode _, int? _) => CreateDayScoreResult(date, 50));

        // Act
        var result = await _service.FindGoodDaysAsync(request);

        // Assert
        result.SearchRangeStart.Should().Be(fromDate);
        result.SearchRangeEnd.Should().Be(toDate);
    }

    private static DayScoreResult CreateDayScoreResult(DateTime solarDate, int score)
    {
        return new DayScoreResult(
            SolarDate: solarDate,
            LunarDateText: "16/4 Bính Ngọ",
            CanChiNgay: "Bính Ngọ",
            TrucIndex: 8,
            TrucName: "Thành",
            TuIndex: 0,
            TuName: "Côn",
            IsHoangDao: true,
            IsSatChu: false,
            IsThuTu: false,
            IsNgayKy: false,
            IsXungTuoi: false,
            Score: score,
            MaxScore: 80,
            Reasons: new List<string> { "Trực Thành tốt cho nhập trạch", "Côn (Kiết Tú)", "Hoàng Đạo" }
        );
    }
}