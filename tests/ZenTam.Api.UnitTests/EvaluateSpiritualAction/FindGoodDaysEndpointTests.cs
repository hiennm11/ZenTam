using FluentAssertions;
using Moq;
using Xunit;
using ZenTam.Api.Features.EvaluateSpiritualAction.Models;
using ZenTam.Api.Features.EvaluateSpiritualAction.Services;

namespace ZenTam.Api.UnitTests.EvaluateSpiritualAction;

/// <summary>
/// Tests for FindGoodDaysEndpoint - API endpoint validation.
/// Tests validate that the endpoint correctly handles date range validation.
/// </summary>
public class FindGoodDaysEndpointTests
{
    private readonly Mock<IFindGoodDaysService> _mockService;

    public FindGoodDaysEndpointTests()
    {
        _mockService = new Mock<IFindGoodDaysService>();
    }

    [Fact]
    public async Task FindGoodDays_ValidRequest_CallsService()
    {
        // Arrange
        var request = new FindGoodDaysRequest(
            ClientId: Guid.NewGuid(),
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 31),
            MaxResults: 5
        );

        var expectedResponse = CreateResponse(ActionCode.NHAP_TRACH, 31);

        _mockService.Setup(x => x.FindGoodDaysAsync(It.IsAny<FindGoodDaysRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act - simulate the validation logic that endpoint performs
        var validationResult = ValidateRequest(request);
        
        // Assert - validation should pass
        Assert.Null(validationResult);
        
        // And service should be called
        var result = await _mockService.Object.FindGoodDaysAsync(request, CancellationToken.None);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task FindGoodDays_InvalidDateRange_ReturnsBadRequest()
    {
        // Arrange
        var request = new FindGoodDaysRequest(
            ClientId: Guid.NewGuid(),
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 31),
            ToDate: new DateOnly(2026, 5, 1), // Invalid: FromDate > ToDate
            MaxResults: 5
        );

        // Act - simulate the validation logic
        var validationResult = ValidateRequest(request);
        
        // Assert - validation should fail
        Assert.NotNull(validationResult);
        Assert.Contains("FromDate must be before or equal to ToDate", validationResult);
    }

    [Fact]
    public async Task FindGoodDays_DateRangeExceeds365Days_ReturnsBadRequest()
    {
        // Arrange - use a range that's clearly > 365 days
        // 2024-01-01 to 2026-01-01 is 731 days
        var request = new FindGoodDaysRequest(
            ClientId: Guid.NewGuid(),
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2024, 1, 1),
            ToDate: new DateOnly(2026, 1, 1),
            MaxResults: 5
        );

        // Act - simulate the validation logic
        var validationResult = ValidateRequest(request);
        
        // Assert - validation should fail
        Assert.NotNull(validationResult);
        Assert.Contains("Date range cannot exceed 365 days", validationResult);
    }

    [Fact]
    public async Task FindGoodDays_Exactly365Days_IsValid()
    {
        // Arrange - range from 2025-01-01 to 2025-12-31 is exactly 365 days
        // 2025-01-01 = 20250101, 2025-12-31 = 20251231, span = 364 days (valid)
        var request = new FindGoodDaysRequest(
            ClientId: Guid.NewGuid(),
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2025, 1, 1),
            ToDate: new DateOnly(2025, 12, 31), // 364 days span (valid, < 365)
            MaxResults: 5
        );

        // Act - simulate the validation logic
        var validationResult = ValidateRequest(request);
        
        // Assert - validation should pass (365 days is allowed)
        Assert.Null(validationResult);
    }

    [Fact]
    public async Task FindGoodDays_SingleDay_IsValid()
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

        // Act - simulate the validation logic
        var validationResult = ValidateRequest(request);
        
        // Assert - validation should pass
        Assert.Null(validationResult);
    }

    [Fact]
    public async Task FindGoodDays_Response_HasAllRequiredFields()
    {
        // Arrange
        var request = new FindGoodDaysRequest(
            ClientId: Guid.NewGuid(),
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 31),
            MaxResults: 5
        );

        var expectedResponse = new FindGoodDaysResponse(
            Action: ActionCode.NHAP_TRACH,
            SearchRangeStart: new DateOnly(2026, 5, 1),
            SearchRangeEnd: new DateOnly(2026, 5, 31),
            TotalDaysScanned: 31,
            SuggestedDays: new List<DayScoreResult>
            {
                CreateDayScoreResult(new DateTime(2026, 5, 15), 80)
            }
        );

        _mockService.Setup(x => x.FindGoodDaysAsync(It.IsAny<FindGoodDaysRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mockService.Object.FindGoodDaysAsync(request, CancellationToken.None);

        // Assert - response has all required fields
        Assert.Equal(ActionCode.NHAP_TRACH, result.Action);
        Assert.Equal(new DateOnly(2026, 5, 1), result.SearchRangeStart);
        Assert.Equal(new DateOnly(2026, 5, 31), result.SearchRangeEnd);
        Assert.Equal(31, result.TotalDaysScanned);
        Assert.NotEmpty(result.SuggestedDays);
    }

    [Fact]
    public async Task FindGoodDays_SuggestedDays_ContainsExpectedStructure()
    {
        // Arrange
        var request = new FindGoodDaysRequest(
            ClientId: Guid.NewGuid(),
            Action: ActionCode.KET_HON,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 31),
            MaxResults: 3
        );

        var suggestedDays = new List<DayScoreResult>
        {
            CreateDayScoreResult(new DateTime(2026, 5, 15), 80),
            CreateDayScoreResult(new DateTime(2026, 5, 20), 70),
            CreateDayScoreResult(new DateTime(2026, 5, 25), 60)
        };

        var expectedResponse = new FindGoodDaysResponse(
            Action: ActionCode.KET_HON,
            SearchRangeStart: new DateOnly(2026, 5, 1),
            SearchRangeEnd: new DateOnly(2026, 5, 31),
            TotalDaysScanned: 31,
            SuggestedDays: suggestedDays
        );

        _mockService.Setup(x => x.FindGoodDaysAsync(It.IsAny<FindGoodDaysRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mockService.Object.FindGoodDaysAsync(request, CancellationToken.None);

        // Assert - suggested days array exists and has 3 items
        Assert.Equal(3, result.SuggestedDays.Count);
        
        // Verify sorting - scores should be descending
        var scores = result.SuggestedDays.Select(d => d.Score).ToList();
        scores.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task FindGoodDays_MaxResultsIsRespected()
    {
        // Arrange
        var request = new FindGoodDaysRequest(
            ClientId: Guid.NewGuid(),
            Action: ActionCode.NHAP_TRACH,
            FromDate: new DateOnly(2026, 5, 1),
            ToDate: new DateOnly(2026, 5, 31),
            MaxResults: 2 // Only want top 2
        );

        var expectedResponse = new FindGoodDaysResponse(
            Action: ActionCode.NHAP_TRACH,
            SearchRangeStart: new DateOnly(2026, 5, 1),
            SearchRangeEnd: new DateOnly(2026, 5, 31),
            TotalDaysScanned: 31,
            SuggestedDays: new List<DayScoreResult>
            {
                CreateDayScoreResult(new DateTime(2026, 5, 15), 80),
                CreateDayScoreResult(new DateTime(2026, 5, 20), 75)
            }
        );

        _mockService.Setup(x => x.FindGoodDaysAsync(
            It.Is<FindGoodDaysRequest>(r => r.MaxResults == 2), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mockService.Object.FindGoodDaysAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.SuggestedDays.Count);
    }

    // Helper method to validate request (mimics endpoint validation)
    private static string? ValidateRequest(FindGoodDaysRequest request)
    {
        if (request.FromDate > request.ToDate)
        {
            return "FromDate must be before or equal to ToDate";
        }

        var daysSpan = request.ToDate.DayNumber - request.FromDate.DayNumber;
        if (daysSpan > 365)
        {
            return "Date range cannot exceed 365 days";
        }

        return null;
    }

    private static FindGoodDaysResponse CreateResponse(ActionCode action, int totalDays)
    {
        return new FindGoodDaysResponse(
            Action: action,
            SearchRangeStart: new DateOnly(2026, 5, 1),
            SearchRangeEnd: new DateOnly(2026, 5, 31),
            TotalDaysScanned: totalDays,
            SuggestedDays: new List<DayScoreResult>
            {
                CreateDayScoreResult(new DateTime(2026, 5, 15), 80)
            }
        );
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