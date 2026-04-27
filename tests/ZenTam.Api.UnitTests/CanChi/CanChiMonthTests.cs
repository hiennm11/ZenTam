namespace ZenTam.Api.UnitTests.CanChi;

using FluentAssertions;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Features.Calendars.Services;

public class CanChiMonthTests
{
    private readonly ICanChiCalculator _calculator = new CanChiCalculator(
        new AmLichCalculator(),
        new SolarTermCalculator(new AmLichCalculator()));

    #region Month Can Chi

    [Fact]
    public void GetCanChiThang_2024_All12Months()
    {
        for (int month = 1; month <= 12; month++)
        {
            var result = _calculator.GetCanChiThang(2024, month, false);
            result.Can.Should().NotBeNullOrEmpty();
            result.Chi.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void GetCanChiThang_Month12_ChiIsHợi()
    {
        // Month 12 always has Chi = Hợi
        var result = _calculator.GetCanChiThang(2024, 12, false);
        result.Chi.Should().Be("Hợi");
    }

    [Fact]
    public void GetCanChiThang_Month1_ChiIsTý()
    {
        // Month 1 always has Chi = Tý
        var result = _calculator.GetCanChiThang(2024, 1, false);
        result.Chi.Should().Be("Tý");
    }

    [Fact]
    public void GetCanChiThang_InvalidMonth_Throws()
    {
        var act = () => _calculator.GetCanChiThang(2024, 13, false);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetCanChiThang_LeapMonth_StillValid()
    {
        // Leap month should still produce valid Can Chi
        var result = _calculator.GetCanChiThang(2024, 2, true);
        result.Can.Should().NotBeNullOrEmpty();
        result.Chi.Should().NotBeNullOrEmpty();
    }

    #endregion
}
