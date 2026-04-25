namespace ZenTam.Api.UnitTests.CanChi;

using FluentAssertions;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.Lunar;

public class NhiThapBatTuTests
{
    private readonly ICanChiCalculator _calculator = new CanChiCalculator(new AmLichCalculator());

    private const int JdnAnchor = 2444235; // 1984-01-01

    #region Happy Paths

    [Fact]
    public void GetNhiThapBatTu_Formula_Verification()
    {
        // Formula: jdn % 28
        // Test that the formula produces consistent results
        for (int jdn = 0; jdn < 100; jdn++)
        {
            var result = _calculator.GetNhiThapBatTu(jdn);
            int expected = jdn % 28;
            result.Should().Be(expected, "for JDN {0}", jdn);
        }
    }

    [Fact]
    public void GetNhiThapBatTu_Cycle_Reset_After_28_Days()
    {
        // Arrange
        var jdn = 5000000;
        var resultAtJdn = _calculator.GetNhiThapBatTu(jdn);
        var resultAtJdnPlus28 = _calculator.GetNhiThapBatTu(jdn + 28);

        // Assert
        resultAtJdn.Should().Be(resultAtJdnPlus28);
    }

    [Fact]
    public void GetNhiThapBatTu_All_28_Tú_Values_Covered()
    {
        // Arrange - test each of the 28 possible values
        var results = new HashSet<int>();
        for (int offset = 0; offset < 28; offset++)
        {
            var result = _calculator.GetNhiThapBatTu(JdnAnchor + offset);
            results.Add(result);
        }

        // Assert - should have all 28 unique values
        results.Count.Should().Be(28);
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public void GetNhiThapBatTu_Boundary_JDN_0_Returns_0()
    {
        // Arrange & Act
        var result = _calculator.GetNhiThapBatTu(0);

        // Assert - 0 % 28 = 0
        result.Should().Be(0);
    }

    [Fact]
    public void GetNhiThapBatTu_Boundary_JDN_27_Returns_27()
    {
        // Arrange & Act
        var result = _calculator.GetNhiThapBatTu(27);

        // Assert - 27 % 28 = 27
        result.Should().Be(27);
    }

    #endregion
}