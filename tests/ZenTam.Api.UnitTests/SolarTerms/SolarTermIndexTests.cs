namespace ZenTam.Api.UnitTests.SolarTerms;

using FluentAssertions;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Features.Calendars.Services;

public class SolarTermIndexTests
{
    private readonly ISolarTermCalculator _calculator = new SolarTermCalculator(new AmLichCalculator());

    [Theory]
    [InlineData("2026-02-04", 0, "Lập Xuân", 1)]    // Lập Xuân boundary
    [InlineData("2026-06-22", 9, "Hạ Chí", 5)]     // Hạ Chí boundary
    [InlineData("2026-12-22", 21, "Đông Chí", 11)] // Đông Chí boundary
    [InlineData("2026-01-20", 23, "Đại Hàn", 12)]  // Đại Hàn boundary
    [InlineData("2026-02-19", 1, "Vũ Thủy", 1)]    // Vũ Thủy
    // Note: 2026-03-05 is near Kinh Trập, 2026-03-20 is near Xuân Phân
    public void GetSolarTermIndex_VariousDates_ReturnsExpectedIndex(
        string dateStr, int expectedIndex, string expectedName, int expectedMonth)
    {
        var date = DateTime.Parse(dateStr);
        
        var index = _calculator.GetSolarTermIndex(date);
        var name = _calculator.GetSolarTermName(date);
        var month = _calculator.GetSolarMonth(date);
        
        index.Should().Be(expectedIndex, $"Date {dateStr} should have index {expectedIndex}");
        name.Should().Be(expectedName, $"Date {dateStr} should have name {expectedName}");
        month.Should().Be(expectedMonth, $"Date {dateStr} should have month {expectedMonth}");
    }

    [Fact]
    public void GetSolarTermIndex_Index_RemainsInRange_0_23()
    {
        var dates = Enumerable.Range(0, 365)
            .Select(i => new DateTime(2026, 1, 1).AddDays(i))
            .ToList();

        foreach (var date in dates)
        {
            var index = _calculator.GetSolarTermIndex(date);
            index.Should().BeInRange(0, 23, $"Date {date:yyyy-MM-dd} should have index 0-23");
        }
    }
}
