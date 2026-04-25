namespace ZenTam.Api.UnitTests.CanChi;

using FluentAssertions;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.Lunar;

public class ThapNhiTrucTests
{
    private readonly ICanChiCalculator _calculator = new CanChiCalculator(new AmLichCalculator());

    private const int JdnAnchor = 2444235; // 1984-01-01

    #region Happy Paths

    [Fact]
    public void GetThapNhiTruc_Formula_Verification()
    {
        // Formula: (jdn + 1) % 12
        // Test that the formula produces consistent results
        for (int jdn = 0; jdn < 100; jdn++)
        {
            var result = _calculator.GetThapNhiTruc(jdn);
            int expected = (jdn + 1) % 12;
            result.Should().Be(expected, "for JDN {0}", jdn);
        }
    }

    [Fact]
    public void GetThapNhiTruc_Cycle_Reset_After_12_Days()
    {
        // Arrange
        var jdn = 5000000;
        var resultAtJdn = _calculator.GetThapNhiTruc(jdn);
        var resultAtJdnPlus12 = _calculator.GetThapNhiTruc(jdn + 12);

        // Assert
        resultAtJdn.Should().Be(resultAtJdnPlus12);
    }

    [Fact]
    public void GetThapNhiTruc_All_12_Trực_Values_Covered()
    {
        // Arrange - test each of the 12 possible values
        var results = new HashSet<int>();
        for (int offset = 0; offset < 12; offset++)
        {
            var result = _calculator.GetThapNhiTruc(JdnAnchor + offset);
            results.Add(result);
        }

        // Assert - should have all 12 unique values
        results.Count.Should().Be(12);
    }

    #endregion
}