namespace ZenTam.Api.UnitTests.CanChi;

using FluentAssertions;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Features.Calendars.Services;

public class CanChiYearTests
{
    private readonly ICanChiCalculator _calculator = new CanChiCalculator(
        new AmLichCalculator(),
        new SolarTermCalculator(new AmLichCalculator()));

    #region Year Can Chi

    [Fact]
    public void GetCanChiNam_2025_ReturnsYearInfo()
    {
        // 2025 is lunar year 2025
        var result = _calculator.GetCanChiNam(2025);
        result.Can.Should().NotBeNullOrEmpty();
        result.Chi.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetCanChiNam_1984_ReturnsGiápTý()
    {
        var result = _calculator.GetCanChiNam(1984);
        result.Can.Should().Be("Giáp");
        result.Chi.Should().Be("Tý");
    }

    [Fact]
    public void GetCanChiNam_60YearCycle()
    {
        var year1984 = _calculator.GetCanChiNam(1984);
        var year2044 = _calculator.GetCanChiNam(2044);
        year1984.Can.Should().Be(year2044.Can);
        year1984.Chi.Should().Be(year2044.Chi);
    }

    #endregion
}
