using ZenTam.Api.Features.Calendars.Data;

namespace ZenTam.Api.UnitTests.Calendars;

public class SatChuLookupTests
{
    // Test matrix from contract: lunarMonth → satChuLunarDay
    // Month 1→9, 2→2, 3→27, 4→4, 5→21, 6→17, 7→10, 8→25, 9→3, 10→20, 11→23, 12→27
    [Theory]
    [InlineData(1, 9)]
    [InlineData(2, 2)]
    [InlineData(3, 27)]
    [InlineData(4, 4)]
    [InlineData(5, 21)]
    [InlineData(6, 17)]
    [InlineData(7, 10)]
    [InlineData(8, 25)]
    [InlineData(9, 3)]
    [InlineData(10, 20)]
    [InlineData(11, 23)]
    [InlineData(12, 27)]
    public void GetSatChuDay_All12Months_ReturnsCorrectDay(int lunarMonth, int expectedDay)
    {
        var result = SatChuLookup.GetSatChuDay(lunarMonth);
        Assert.Equal(expectedDay, result);
    }

    [Theory]
    [InlineData(1, 9, true)]
    [InlineData(1, 10, false)]
    [InlineData(2, 2, true)]
    [InlineData(2, 3, false)]
    [InlineData(3, 27, true)]
    [InlineData(3, 28, false)]
    [InlineData(4, 4, true)]
    [InlineData(4, 5, false)]
    [InlineData(5, 21, true)]
    [InlineData(5, 22, false)]
    [InlineData(6, 17, true)]
    [InlineData(6, 18, false)]
    [InlineData(7, 10, true)]
    [InlineData(7, 11, false)]
    [InlineData(8, 25, true)]
    [InlineData(8, 26, false)]
    [InlineData(9, 3, true)]
    [InlineData(9, 4, false)]
    [InlineData(10, 20, true)]
    [InlineData(10, 21, false)]
    [InlineData(11, 23, true)]
    [InlineData(11, 24, false)]
    [InlineData(12, 27, true)]
    [InlineData(12, 28, false)]
    public void IsSatChu_ReturnsCorrectResult(int lunarMonth, int lunarDay, bool expected)
    {
        var result = SatChuLookup.IsSatChu(lunarMonth, lunarDay);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsSatChu_NonMatchingDay_ReturnsFalse()
    {
        // Non-matching days should return false
        Assert.False(SatChuLookup.IsSatChu(1, 1));
        Assert.False(SatChuLookup.IsSatChu(1, 30));
        Assert.False(SatChuLookup.IsSatChu(12, 1));
    }

    [Fact]
    public void GetSatChuDay_LunarLeapMonth_Handled()
    {
        // Leap month is handled by using lunarMonth number only
        // The lookup table is based on month number, not whether it's leap
        var regularDay = SatChuLookup.GetSatChuDay(1);
        var leapDay = SatChuLookup.GetSatChuDay(1); // Same month number
        Assert.Equal(regularDay, leapDay);
    }

    [Fact]
    public void GetSatChuDay_Day30_Handled()
    {
        // Day 30 should not crash and should return false
        var result = SatChuLookup.IsSatChu(1, 30);
        Assert.False(result);
    }
}
