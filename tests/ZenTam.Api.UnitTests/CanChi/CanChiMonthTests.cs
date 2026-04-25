namespace ZenTam.Api.UnitTests.CanChi;

using FluentAssertions;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.CanChi.Models;

public class CanChiMonthTests
{
    private readonly ICanChiCalculator _calculator = new CanChiCalculator();

    #region Checkpoint Test

    [Fact]
    public void GetCanChiThang_1984_Month1_Returns_GiápTý()
    {
        // 1984 is Giáp year (canIndex=0), month 1
        var result = _calculator.GetCanChiThang(1984, 1, isLeapMonth: false);
        result.Should().Be(new CanChiMonth("Giáp", "Tý"));
    }

    #endregion

    #region Month Can Depends on Year Can

    [Fact]
    public void GetCanChiThang_Giáp_Year_Month2_Returns_BínhSửu()
    {
        // Giáp year (canIndex=0), month 2
        // Row 0: [0, 2, 4, 6, 8, 0, 2, 4, 6, 8, 0, 2]
        // Month 2 (index 1) = 2 = Bính
        var result = _calculator.GetCanChiThang(1984, 2, isLeapMonth: false);
        result.Should().Be(new CanChiMonth("Bính", "Sửu"));
    }

    [Fact]
    public void GetCanChiThang_Ất_Year_Month1_Returns_ẤtTý()
    {
        // Ất year (canIndex=1), month 1
        // Row 1: [1, 3, 5, 7, 9, 1, 3, 5, 7, 9, 1, 3]
        // Month 1 (index 0) = 1 = Ất
        var result = _calculator.GetCanChiThang(1985, 1, isLeapMonth: false);
        result.Should().Be(new CanChiMonth("Ất", "Tý"));
    }

    #endregion

    #region Leap Month Handling

    [Fact]
    public void GetCanChiThang_Leap_Month_Same_CanChi_As_Regular()
    {
        var regularResult = _calculator.GetCanChiThang(1984, 5, isLeapMonth: false);
        var leapResult = _calculator.GetCanChiThang(1984, 5, isLeapMonth: true);

        leapResult.Should().Be(regularResult);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GetCanChiThang_Month12_Chi_Is_Hợi()
    {
        var result = _calculator.GetCanChiThang(1984, 12, isLeapMonth: false);
        result.Chi.Should().Be("Hợi");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void GetCanChiThang_Invalid_Month_Throws(int invalidMonth)
    {
        var act = () => _calculator.GetCanChiThang(1984, invalidMonth, isLeapMonth: false);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion
}