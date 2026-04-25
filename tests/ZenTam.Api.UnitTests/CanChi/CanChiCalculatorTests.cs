namespace ZenTam.Api.UnitTests.CanChi;

using FluentAssertions;
using ZenTam.Api.Common.CanChi;

public class CanChiCalculatorTests
{
    private readonly ICanChiCalculator _calculator = new CanChiCalculator();

    private const int JdnAnchor = 2444235; // 1984-01-01 = Giáp Tý

    #region Checkpoint Tests

    [Fact]
    public void GetCanChiNgay_Anchor_JDN_Returns_GiápTý_Checkpoint()
    {
        // CHECKPOINT: GetCanChiNgay(2444235) → ("Giáp", "Tý")
        var result = _calculator.GetCanChiNgay(JdnAnchor);
        result.Can.Should().Be("Giáp");
        result.Chi.Should().Be("Tý");
    }

    [Fact]
    public void GetCanChiNgay_Cycle_Completes_Resets_To_GiápTý()
    {
        // +60 days should reset to Giáp Tý
        var result = _calculator.GetCanChiNgay(JdnAnchor + 60);
        result.Can.Should().Be("Giáp");
        result.Chi.Should().Be("Tý");
    }

    #endregion

    #region Day Lookup Tests

    [Fact]
    public void GetCanChiNgay_Anchor_Plus_1_Returns_ẤtSửu()
    {
        var result = _calculator.GetCanChiNgay(JdnAnchor + 1);
        result.Can.Should().Be("Ất");
        result.Chi.Should().Be("Sửu");
    }

    [Fact]
    public void GetCanChiNgay_Anchor_Plus_59_Returns_QuýHợi()
    {
        var result = _calculator.GetCanChiNgay(JdnAnchor + 59);
        result.Can.Should().Be("Quý");
        result.Chi.Should().Be("Hợi");
    }

    #endregion

    #region GetCanChiGio Tests

    [Fact]
    public void GetCanChiGio_Returns_Valid_CanChi()
    {
        // Just verify it returns valid results
        var result = _calculator.GetCanChiGio(JdnAnchor, 23);
        result.Can.Should().NotBeNullOrEmpty();
        result.Chi.Should().NotBeNullOrEmpty();
    }

    #endregion
}