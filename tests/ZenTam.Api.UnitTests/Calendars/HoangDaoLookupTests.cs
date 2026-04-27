using ZenTam.Api.Features.Calendars.Data;
using ZenTam.Api.Features.Calendars.Models;

namespace ZenTam.Api.UnitTests.Calendars;

public class HoangDaoLookupTests
{
    // All 12 chi names
    private static readonly string[] AllChis =
        ["Tý", "Sửu", "Dần", "Mão", "Thìn", "Tỵ", "Ngọ", "Mùi", "Thân", "Dậu", "Tuất", "Hợi"];

    // Pattern A: Tý,Sửu,Ngọ,Mùi,Dậu,Hợi (Can ends with Tý/Ngọ/Dậu - canIndex 0,2,7 + chi 0,6,9)
    [Fact]
    public void GetHoangDao_PatternA_GiapTy_ReturnsCorrectHours()
    {
        // Giáp Tý: canIndex=0, chiIndex=0
        var result = HoangDaoLookup.GetHoangDao(0, 0);
        
        Assert.True(result.IsHoangDao);
        Assert.Equal(6, result.HoangDaoHours.Count);
        Assert.Equal(6, result.HacDaoHours.Count);
        Assert.Contains("Tý", result.HoangDaoHours);
        Assert.Contains("Sửu", result.HoangDaoHours);
        Assert.Contains("Ngọ", result.HoangDaoHours);
        Assert.Contains("Mùi", result.HoangDaoHours);
        Assert.Contains("Dậu", result.HoangDaoHours);
        Assert.Contains("Hợi", result.HoangDaoHours);
    }

    [Fact]
    public void GetHoangDao_PatternA_BinhNgo_ReturnsCorrectHours()
    {
        // Bính Ngọ: canIndex=2, chiIndex=6
        var result = HoangDaoLookup.GetHoangDao(2, 6);
        
        Assert.True(result.IsHoangDao);
        Assert.Equal(6, result.HoangDaoHours.Count);
        Assert.Contains("Tý", result.HoangDaoHours);
        Assert.Contains("Sửu", result.HoangDaoHours);
        Assert.Contains("Ngọ", result.HoangDaoHours);
        Assert.Contains("Mùi", result.HoangDaoHours);
        Assert.Contains("Dậu", result.HoangDaoHours);
        Assert.Contains("Hợi", result.HoangDaoHours);
    }

    [Fact]
    public void GetHoangDao_PatternA_TanDau_ReturnsCorrectHours()
    {
        // Tân Dậu: canIndex=7, chiIndex=9
        var result = HoangDaoLookup.GetHoangDao(7, 9);
        
        Assert.True(result.IsHoangDao);
        Assert.Equal(6, result.HoangDaoHours.Count);
        Assert.Contains("Tý", result.HoangDaoHours);
        Assert.Contains("Sửu", result.HoangDaoHours);
        Assert.Contains("Ngọ", result.HoangDaoHours);
        Assert.Contains("Mùi", result.HoangDaoHours);
        Assert.Contains("Dậu", result.HoangDaoHours);
        Assert.Contains("Hợi", result.HoangDaoHours);
    }

    // Pattern B: Dần,Mão,Ngọ,Mùi,Thân,Tuất (Can ends with Sửu/Mùi/Thân - canIndex 1,3,6 + chi 1,7,8)
    [Fact]
    public void GetHoangDao_PatternB_AtSuu_ReturnsCorrectHours()
    {
        // Ất Sửu: canIndex=1, chiIndex=1
        var result = HoangDaoLookup.GetHoangDao(1, 1);
        
        Assert.True(result.IsHoangDao);
        Assert.Equal(6, result.HoangDaoHours.Count);
        Assert.Contains("Dần", result.HoangDaoHours);
        Assert.Contains("Mão", result.HoangDaoHours);
        Assert.Contains("Ngọ", result.HoangDaoHours);
        Assert.Contains("Mùi", result.HoangDaoHours);
        Assert.Contains("Thân", result.HoangDaoHours);
        Assert.Contains("Tuất", result.HoangDaoHours);
    }

    [Fact]
    public void GetHoangDao_PatternB_DinhMui_ReturnsCorrectHours()
    {
        // Đinh Mùi: canIndex=3, chiIndex=7
        var result = HoangDaoLookup.GetHoangDao(3, 7);
        
        Assert.True(result.IsHoangDao);
        Assert.Equal(6, result.HoangDaoHours.Count);
        Assert.Contains("Dần", result.HoangDaoHours);
        Assert.Contains("Mão", result.HoangDaoHours);
        Assert.Contains("Ngọ", result.HoangDaoHours);
        Assert.Contains("Mùi", result.HoangDaoHours);
        Assert.Contains("Thân", result.HoangDaoHours);
        Assert.Contains("Tuất", result.HoangDaoHours);
    }

    [Fact]
    public void GetHoangDao_PatternB_CanhThan_ReturnsCorrectHours()
    {
        // Canh Thân: canIndex=6, chiIndex=8
        var result = HoangDaoLookup.GetHoangDao(6, 8);
        
        Assert.True(result.IsHoangDao);
        Assert.Equal(6, result.HoangDaoHours.Count);
        Assert.Contains("Dần", result.HoangDaoHours);
        Assert.Contains("Mão", result.HoangDaoHours);
        Assert.Contains("Ngọ", result.HoangDaoHours);
        Assert.Contains("Mùi", result.HoangDaoHours);
        Assert.Contains("Thân", result.HoangDaoHours);
        Assert.Contains("Tuất", result.HoangDaoHours);
    }

    // Pattern C: Tý,Sửu,Thìn,Tỵ,Dậu,Hợi (Can ends with Dần/Tỵ/Hợi - canIndex 4,5,8,9 + chi 2,5,11)
    [Fact]
    public void GetHoangDao_PatternC_MauDan_ReturnsCorrectHours()
    {
        // Mậu Dần: canIndex=4, chiIndex=2
        var result = HoangDaoLookup.GetHoangDao(4, 2);
        
        Assert.True(result.IsHoangDao);
        Assert.Equal(6, result.HoangDaoHours.Count);
        Assert.Contains("Tý", result.HoangDaoHours);
        Assert.Contains("Sửu", result.HoangDaoHours);
        Assert.Contains("Thìn", result.HoangDaoHours);
        Assert.Contains("Tỵ", result.HoangDaoHours);
        Assert.Contains("Dậu", result.HoangDaoHours);
        Assert.Contains("Hợi", result.HoangDaoHours);
    }

    [Fact]
    public void GetHoangDao_PatternC_KyTy_ReturnsCorrectHours()
    {
        // Kỷ Tỵ: canIndex=5, chiIndex=5
        var result = HoangDaoLookup.GetHoangDao(5, 5);
        
        Assert.True(result.IsHoangDao);
        Assert.Equal(6, result.HoangDaoHours.Count);
        Assert.Contains("Tý", result.HoangDaoHours);
        Assert.Contains("Sửu", result.HoangDaoHours);
        Assert.Contains("Thìn", result.HoangDaoHours);
        Assert.Contains("Tỵ", result.HoangDaoHours);
        Assert.Contains("Dậu", result.HoangDaoHours);
        Assert.Contains("Hợi", result.HoangDaoHours);
    }

    [Fact]
    public void GetHoangDao_PatternC_NhamHoi_ReturnsCorrectHours()
    {
        // Nhâm Hợi: canIndex=8, chiIndex=11
        var result = HoangDaoLookup.GetHoangDao(8, 11);
        
        Assert.True(result.IsHoangDao);
        Assert.Equal(6, result.HoangDaoHours.Count);
        Assert.Contains("Tý", result.HoangDaoHours);
        Assert.Contains("Sửu", result.HoangDaoHours);
        Assert.Contains("Thìn", result.HoangDaoHours);
        Assert.Contains("Tỵ", result.HoangDaoHours);
        Assert.Contains("Dậu", result.HoangDaoHours);
        Assert.Contains("Hợi", result.HoangDaoHours);
    }

    // Pattern D: Dần,Mão,Ngọ,Mùi,Thân,Tuất (same as B - Can ends with Mão/Tuất - canIndex 1,3,6 + chi 3,10)
    [Fact]
    public void GetHoangDao_PatternD_AtMao_ReturnsCorrectHours()
    {
        // Ất Mão: canIndex=1, chiIndex=3
        var result = HoangDaoLookup.GetHoangDao(1, 3);
        
        Assert.True(result.IsHoangDao);
        Assert.Equal(6, result.HoangDaoHours.Count);
        Assert.Contains("Dần", result.HoangDaoHours);
        Assert.Contains("Mão", result.HoangDaoHours);
        Assert.Contains("Ngọ", result.HoangDaoHours);
        Assert.Contains("Mùi", result.HoangDaoHours);
        Assert.Contains("Thân", result.HoangDaoHours);
        Assert.Contains("Tuất", result.HoangDaoHours);
    }

    [Fact]
    public void GetHoangDao_PatternD_CanhTuat_ReturnsCorrectHours()
    {
        // Canh Tuất: canIndex=6, chiIndex=10
        var result = HoangDaoLookup.GetHoangDao(6, 10);
        
        Assert.True(result.IsHoangDao);
        Assert.Equal(6, result.HoangDaoHours.Count);
        Assert.Contains("Dần", result.HoangDaoHours);
        Assert.Contains("Mão", result.HoangDaoHours);
        Assert.Contains("Ngọ", result.HoangDaoHours);
        Assert.Contains("Mùi", result.HoangDaoHours);
        Assert.Contains("Thân", result.HoangDaoHours);
        Assert.Contains("Tuất", result.HoangDaoHours);
    }

    // Pattern E: Tý,Sửu,Ngọ,Mùi,Dậu,Hợi (same as A - Can ends with Thìn - canIndex 4 + chi 4)
    [Fact]
    public void GetHoangDao_PatternE_MauThin_ReturnsCorrectHours()
    {
        // Mậu Thìn: canIndex=4, chiIndex=4
        var result = HoangDaoLookup.GetHoangDao(4, 4);
        
        Assert.True(result.IsHoangDao);
        Assert.Equal(6, result.HoangDaoHours.Count);
        Assert.Contains("Tý", result.HoangDaoHours);
        Assert.Contains("Sửu", result.HoangDaoHours);
        Assert.Contains("Ngọ", result.HoangDaoHours);
        Assert.Contains("Mùi", result.HoangDaoHours);
        Assert.Contains("Dậu", result.HoangDaoHours);
        Assert.Contains("Hợi", result.HoangDaoHours);
    }

    // Top hours tests
    [Fact]
    public void GetHoangDao_PatternA_TopHours_ReturnsCorrectTop3()
    {
        var result = HoangDaoLookup.GetHoangDao(0, 0);
        
        Assert.Equal(3, result.TopHours.Count);
        Assert.Contains("Tý", result.TopHours);
        Assert.Contains("Ngọ", result.TopHours);
        Assert.Contains("Mùi", result.TopHours);
    }

    [Fact]
    public void GetHoangDao_PatternB_TopHours_ReturnsCorrectTop3()
    {
        var result = HoangDaoLookup.GetHoangDao(1, 1);
        
        Assert.Equal(3, result.TopHours.Count);
        Assert.Contains("Ngọ", result.TopHours);
        Assert.Contains("Mão", result.TopHours);
        Assert.Contains("Thân", result.TopHours);
    }

    [Fact]
    public void GetHoangDao_PatternC_TopHours_ReturnsCorrectTop3()
    {
        var result = HoangDaoLookup.GetHoangDao(4, 2);
        
        Assert.Equal(3, result.TopHours.Count);
        Assert.Contains("Tý", result.TopHours);
        Assert.Contains("Thìn", result.TopHours);
        Assert.Contains("Dậu", result.TopHours);
    }

    // Hoàng Đạo and Hắc Đạo are mutually exclusive
    [Fact]
    public void GetHoangDao_HoangDaoAndHacDao_AreMutuallyExclusive()
    {
        var result = HoangDaoLookup.GetHoangDao(0, 0);
        
        var overlap = result.HoangDaoHours.Intersect(result.HacDaoHours).ToList();
        Assert.Empty(overlap);
    }

    [Fact]
    public void GetHoangDao_HoangDaoAndHacDao_CoverAll12Hours()
    {
        var result = HoangDaoLookup.GetHoangDao(0, 0);
        
        var union = result.HoangDaoHours.Union(result.HacDaoHours).ToList();
        Assert.Equal(12, union.Count);
    }

    [Fact]
    public void GetHoangDao_TopHours_AreSubsetOfHoangDaoHours()
    {
        var result = HoangDaoLookup.GetHoangDao(0, 0);
        
        Assert.True(result.TopHours.All(h => result.HoangDaoHours.Contains(h)));
    }

    // Non-matching combination returns empty
    [Fact]
    public void GetHoangDao_NonMatchingCombination_ReturnsNotHoangDao()
    {
        // This combination should not match any pattern
        var result = HoangDaoLookup.GetHoangDao(0, 1); // Giáp Sửu
        
        Assert.False(result.IsHoangDao);
        Assert.Empty(result.HoangDaoHours);
        Assert.Empty(result.TopHours);
        Assert.Equal(12, result.HacDaoHours.Count);
    }
}
