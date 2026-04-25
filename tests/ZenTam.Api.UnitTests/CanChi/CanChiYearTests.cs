namespace ZenTam.Api.UnitTests.CanChi;

using FluentAssertions;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.CanChi.Models;
using ZenTam.Api.Common.Lunar;

public class CanChiYearTests
{
    private readonly ICanChiCalculator _calculator = new CanChiCalculator(new AmLichCalculator());

    #region Core Checkpoint Tests

    [Fact]
    public void GetCanChiNam_1984_Returns_GiápTý_Checkpoint()
    {
        // CHECKPOINT: GetCanChiNam(1984) → ("Giáp", "Tý")
        var result = _calculator.GetCanChiNam(1984);
        result.Should().Be(new CanChiYear("Giáp", "Tý"));
    }

    [Fact]
    public void GetCanChiNam_2026_Returns_Correct()
    {
        // (2026+6)%10=2="Bính", (2026+8)%12=6="Ngọ"
        var result = _calculator.GetCanChiNam(2026);
        result.Can.Should().Be("Bính");
        result.Chi.Should().Be("Ngọ");
    }

    #endregion

    #region Formula Verification Tests

    [Theory]
    [InlineData(1984, "Giáp", 0, "Tý", 0)]
    [InlineData(1985, "Ất", 1, "Sửu", 1)]
    [InlineData(1986, "Bính", 2, "Dần", 2)]
    [InlineData(1987, "Đinh", 3, "Mão", 3)]
    [InlineData(1988, "Mậu", 4, "Thìn", 4)]
    [InlineData(1989, "Kỷ", 5, "Tỵ", 5)]
    [InlineData(1990, "Canh", 6, "Ngọ", 6)]
    [InlineData(1991, "Tân", 7, "Mùi", 7)]
    [InlineData(1992, "Nhâm", 8, "Thân", 8)]
    [InlineData(1993, "Quý", 9, "Dậu", 9)]
    public void GetCanChiNam_Formula_Verification(int year, string expectedCan, int expectedCanIdx, string expectedChi, int expectedChiIdx)
    {
        var result = _calculator.GetCanChiNam(year);
        
        // Verify formula: Can = (year + 6) % 10
        int calculatedCanIdx = (year + 6) % 10;
        calculatedCanIdx.Should().Be(expectedCanIdx);
        
        // Verify formula: Chi = (year + 8) % 12
        int calculatedChiIdx = (year + 8) % 12;
        calculatedChiIdx.Should().Be(expectedChiIdx);
        
        // Verify result matches
        result.Can.Should().Be(expectedCan);
        result.Chi.Should().Be(expectedChi);
    }

    #endregion
}