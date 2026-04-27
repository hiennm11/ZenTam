using ZenTam.Api.Features.Calendars.Data;

namespace ZenTam.Api.UnitTests.Calendars;

public class ThuTuLookupTests
{
    // Test matrix from contract: lunarMonth → forbidden chi pairs
    // 1: Tý/Ngọ [0,6], 2: Sửu/Mùi [1,7], 3: Dần/Thân [2,8], 4: Mão/Dậu [3,9]
    // 5: Thìn/Tuất [4,10], 6: Tỵ/Hợi [5,11], 7: Ngọ/Tý [6,0], 8: Mùi/Sửu [7,1]
    // 9: Thân/Dần [8,2], 10: Dậu/Mão [9,3], 11: Tuất/Thìn [10,4], 12: Hợi/Tỵ [11,5]
    
    [Theory]
    [InlineData(1, 0, true)]
    [InlineData(1, 6, true)]
    [InlineData(1, 1, false)]
    [InlineData(1, 2, false)]
    public void IsThuTu_Month1_ReturnsCorrectResult(int lunarMonth, int chiIndex, bool expected)
    {
        var result = ThuTuLookup.IsThuTu(lunarMonth, chiIndex);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(2, 1, true)]
    [InlineData(2, 7, true)]
    [InlineData(2, 0, false)]
    [InlineData(2, 6, false)]
    public void IsThuTu_Month2_ReturnsCorrectResult(int lunarMonth, int chiIndex, bool expected)
    {
        var result = ThuTuLookup.IsThuTu(lunarMonth, chiIndex);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(3, 2, true)]
    [InlineData(3, 8, true)]
    [InlineData(3, 0, false)]
    public void IsThuTu_Month3_ReturnsCorrectResult(int lunarMonth, int chiIndex, bool expected)
    {
        var result = ThuTuLookup.IsThuTu(lunarMonth, chiIndex);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(4, 3, true)]
    [InlineData(4, 9, true)]
    [InlineData(4, 0, false)]
    public void IsThuTu_Month4_ReturnsCorrectResult(int lunarMonth, int chiIndex, bool expected)
    {
        var result = ThuTuLookup.IsThuTu(lunarMonth, chiIndex);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(5, 4, true)]
    [InlineData(5, 10, true)]
    [InlineData(5, 0, false)]
    public void IsThuTu_Month5_ReturnsCorrectResult(int lunarMonth, int chiIndex, bool expected)
    {
        var result = ThuTuLookup.IsThuTu(lunarMonth, chiIndex);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(6, 5, true)]
    [InlineData(6, 11, true)]
    [InlineData(6, 0, false)]
    public void IsThuTu_Month6_ReturnsCorrectResult(int lunarMonth, int chiIndex, bool expected)
    {
        var result = ThuTuLookup.IsThuTu(lunarMonth, chiIndex);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(7, 6, true)]
    [InlineData(7, 0, true)]
    [InlineData(7, 1, false)]
    public void IsThuTu_Month7_ReturnsCorrectResult(int lunarMonth, int chiIndex, bool expected)
    {
        var result = ThuTuLookup.IsThuTu(lunarMonth, chiIndex);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(8, 7, true)]
    [InlineData(8, 1, true)]
    [InlineData(8, 0, false)]
    public void IsThuTu_Month8_ReturnsCorrectResult(int lunarMonth, int chiIndex, bool expected)
    {
        var result = ThuTuLookup.IsThuTu(lunarMonth, chiIndex);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(9, 8, true)]
    [InlineData(9, 2, true)]
    [InlineData(9, 0, false)]
    public void IsThuTu_Month9_ReturnsCorrectResult(int lunarMonth, int chiIndex, bool expected)
    {
        var result = ThuTuLookup.IsThuTu(lunarMonth, chiIndex);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(10, 9, true)]
    [InlineData(10, 3, true)]
    [InlineData(10, 0, false)]
    public void IsThuTu_Month10_ReturnsCorrectResult(int lunarMonth, int chiIndex, bool expected)
    {
        var result = ThuTuLookup.IsThuTu(lunarMonth, chiIndex);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(11, 10, true)]
    [InlineData(11, 4, true)]
    [InlineData(11, 0, false)]
    public void IsThuTu_Month11_ReturnsCorrectResult(int lunarMonth, int chiIndex, bool expected)
    {
        var result = ThuTuLookup.IsThuTu(lunarMonth, chiIndex);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(12, 11, true)]
    [InlineData(12, 5, true)]
    [InlineData(12, 0, false)]
    public void IsThuTu_Month12_ReturnsCorrectResult(int lunarMonth, int chiIndex, bool expected)
    {
        var result = ThuTuLookup.IsThuTu(lunarMonth, chiIndex);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    public void GetForbiddenChi_All12Months_ReturnsExactly2Chi(int lunarMonth)
    {
        var result = ThuTuLookup.GetForbiddenChi(lunarMonth);
        Assert.Equal(2, result.Length);
    }

    [Theory]
    [InlineData(1, 0, 6)]
    [InlineData(2, 1, 7)]
    [InlineData(3, 2, 8)]
    [InlineData(4, 3, 9)]
    [InlineData(5, 4, 10)]
    [InlineData(6, 5, 11)]
    [InlineData(7, 6, 0)]
    [InlineData(8, 7, 1)]
    [InlineData(9, 8, 2)]
    [InlineData(10, 9, 3)]
    [InlineData(11, 10, 4)]
    [InlineData(12, 11, 5)]
    public void GetForbiddenChi_All12Months_ReturnsCorrectPairs(int lunarMonth, int expectedFirst, int expectedSecond)
    {
        var result = ThuTuLookup.GetForbiddenChi(lunarMonth);
        Assert.Equal(expectedFirst, result[0]);
        Assert.Equal(expectedSecond, result[1]);
    }

    [Fact]
    public void IsThuTu_NonForbiddenChi_ReturnsFalse()
    {
        // Non-matching chi should return false
        Assert.False(ThuTuLookup.IsThuTu(1, 3)); // Month 1, Mão (not Tý or Ngọ)
        Assert.False(ThuTuLookup.IsThuTu(6, 0)); // Month 6, Tý (not Tỵ or Hợi)
    }
}
