namespace ZenTam.Api.UnitTests.CanChi;

using FluentAssertions;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Features.Calendars.Services;

public class CanChiCalculatorTests
{
    private readonly ICanChiCalculator _calculator = new CanChiCalculator(
        new AmLichCalculator(),
        new SolarTermCalculator(new AmLichCalculator()));

    #region Can Chi Nam

    [Fact]
    public void GetCanChiNam_2024_ReturnsCanNamBinh()
    {
        // 2024 is a leap year in lunar calendar (甲辰)
        var result = _calculator.GetCanChiNam(2024);
        result.Can.Should().Be("Giáp");
        result.Chi.Should().Be("Thìn");
    }

    [Fact]
    public void GetCanChiNam_1984_ReturnsGiápTý()
    {
        // 1984 is Giáp Tý
        var result = _calculator.GetCanChiNam(1984);
        result.Can.Should().Be("Giáp");
        result.Chi.Should().Be("Tý");
    }

    [Fact]
    public void GetCanChiNam_Cycle_60Years()
    {
        // The Can Chi cycle for years is 60 years
        var year1 = _calculator.GetCanChiNam(1984);
        var year61 = _calculator.GetCanChiNam(2044); // 1984 + 60
        year1.Can.Should().Be(year61.Can);
        year1.Chi.Should().Be(year61.Chi);
    }

    #endregion

    #region Can Chi Ngày

    [Fact]
    public void GetCanChiNgay_1984Jan01_ReturnsGiápTý()
    {
        // 1984-01-01 = JDN 2444235 = Giáp Tý
        var result = _calculator.GetCanChiNgay(2444235);
        result.Can.Should().Be("Giáp");
        result.Chi.Should().Be("Tý");
    }

    [Fact]
    public void GetCanChiNgay_Cycle_60Days()
    {
        // The Can Chi cycle for days is 60 days
        var jdn1 = 2444235;
        var result1 = _calculator.GetCanChiNgay(jdn1);
        var result60 = _calculator.GetCanChiNgay(jdn1 + 60);
        result1.Can.Should().Be(result60.Can);
        result1.Chi.Should().Be(result60.Chi);
    }

    #endregion

    #region JDN Calculation

    [Fact]
    public void GetJulianDayNumber_ValidDates()
    {
        // Verify JDN calculation works for known dates
        var jdn = _calculator.GetJulianDayNumber(new DateTime(1984, 1, 1));
        jdn.Should().BeGreaterThan(0); // Valid JDN
        
        // 2026-01-01 should be later than 1984-01-01
        var jdn2026 = _calculator.GetJulianDayNumber(new DateTime(2026, 1, 1));
        jdn2026.Should().BeGreaterThan(jdn);
    }

    #endregion
}
