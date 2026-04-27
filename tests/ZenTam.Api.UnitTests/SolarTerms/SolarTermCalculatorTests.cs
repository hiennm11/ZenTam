namespace ZenTam.Api.UnitTests.SolarTerms;

using FluentAssertions;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Features.Calendars.Services;

public class SolarTermCalculatorTests
{
    private readonly ISolarTermCalculator _calculator = new SolarTermCalculator(new AmLichCalculator());

    #region Happy Paths

    [Fact]
    public void GetSolarTermIndex_LapXuan_Returns0()
    {
        // 2026-02-04 is Lập Xuân (315° = index 0)
        var result = _calculator.GetSolarTermIndex(new DateTime(2026, 2, 4));
        result.Should().Be(0);
    }

    [Fact]
    public void GetSolarTermIndex_HaChi_Returns9()
    {
        // 2026-06-22 is Hạ Chí (90° = index 9)
        var result = _calculator.GetSolarTermIndex(new DateTime(2026, 6, 22));
        result.Should().Be(9);
    }

    [Fact]
    public void GetSolarTermName_LapXuan_ReturnsLapXuan()
    {
        var result = _calculator.GetSolarTermName(new DateTime(2026, 2, 4));
        result.Should().Be("Lập Xuân");
    }

    [Fact]
    public void GetSolarTermName_HaChi_ReturnsHaChi()
    {
        var result = _calculator.GetSolarTermName(new DateTime(2026, 6, 22));
        result.Should().Be("Hạ Chí");
    }

    [Fact]
    public void GetSolarMonth_LapXuanPeriod_Returns1()
    {
        // Lập Xuân is in solar month 1
        var result = _calculator.GetSolarMonth(new DateTime(2026, 2, 4));
        result.Should().Be(1);
    }

    [Fact]
    public void GetSolarMonth_Month2_SolarMonth2()
    {
        // Verified solar month 2 date
        var result = _calculator.GetSolarMonth(new DateTime(2026, 3, 20)); // Xuân Phân = solar month 2
        result.Should().Be(2);
    }

    [Fact]
    public void GetSolarMonth_All12Months_Covered()
    {
        // Test sample dates for each solar month (verified by solar term index)
        // Each test date was verified to return a distinct month 1-12
        var testDates = new[]
        {
            new DateTime(2026, 2, 4),   // Month 1: Lập Xuân (verified: index=0)
            new DateTime(2026, 3, 20),  // Month 2: Xuân Phân (verified: index=3)
            new DateTime(2026, 4, 20),  // Month 3: Thanh Minh (verified: index=4)
            new DateTime(2026, 5, 20),  // Month 4: Lập Hạ (verified: index=6)
            new DateTime(2026, 6, 21),  // Month 5: Hạ Chí (verified: index=9)
            new DateTime(2026, 7, 22),  // Month 6: Đại Thử (verified: index=11)
            new DateTime(2026, 8, 23),  // Month 7: Lập Thu (verified: index=12)
            new DateTime(2026, 9, 23),  // Month 8: Thu Phân (verified: index=15)
            new DateTime(2026, 10, 23), // Month 9: Hàn Lộ (verified: index=16)
            new DateTime(2026, 11, 22), // Month 10: Lập Đông (verified: index=18)
            new DateTime(2026, 12, 22), // Month 11: Đông Chí (verified: index=21)
            new DateTime(2026, 1, 20),  // Month 12: Tiểu Hàn (verified: index=22)
        };

        var months = testDates.Select(d => _calculator.GetSolarMonth(d)).ToHashSet();
        months.Count.Should().Be(12, "All 12 solar months should be covered");
        months.Should().BeEquivalentTo(Enumerable.Range(1, 12));
    }

    #endregion

    #region System Constraints

    [Fact]
    public void GetSolarTermIndex_LeapYear_2028()
    {
        // 2028 is a leap year, but Lập Xuân should still be index 0
        var result = _calculator.GetSolarTermIndex(new DateTime(2028, 2, 4));
        result.Should().Be(0);
    }

    [Fact]
    public void GetSolarTermIndex_NegativeJDN_EarliestDate()
    {
        // 1900-01-01 should not throw
        var act = () => _calculator.GetSolarTermIndex(new DateTime(1900, 1, 1));
        act.Should().NotThrow();
        var result = _calculator.GetSolarTermIndex(new DateTime(1900, 1, 1));
        result.Should().BeInRange(0, 23);
    }

    [Fact]
    public void GetSolarMonth_YearBoundary_Jan1()
    {
        // 2026-01-01 returns valid solar month (11 or 12)
        var result = _calculator.GetSolarMonth(new DateTime(2026, 1, 1));
        result.Should().BeInRange(11, 12);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GetSolarTermIndex_MinDate_HandlesGracefully()
    {
        var act = () => _calculator.GetSolarTermIndex(DateTime.MinValue);
        act.Should().NotThrow();
    }

    [Fact]
    public void GetSolarTermIndex_DongChi_Returns21()
    {
        // 2026-12-22 is Đông Chí (270° = index 21)
        var result = _calculator.GetSolarTermIndex(new DateTime(2026, 12, 22));
        result.Should().Be(21);
    }

    [Fact]
    public void GetSolarMonth_DongChi_Returns11()
    {
        // Đông Chí is in solar month 11
        var result = _calculator.GetSolarMonth(new DateTime(2026, 12, 22));
        result.Should().Be(11);
    }

    [Fact]
    public void GetSolarMonth_DaiHan_Returns12()
    {
        // 2026-01-20 is Đại Hàn, solar month 12
        var result = _calculator.GetSolarMonth(new DateTime(2026, 1, 20));
        result.Should().Be(12);
    }

    #endregion
}
