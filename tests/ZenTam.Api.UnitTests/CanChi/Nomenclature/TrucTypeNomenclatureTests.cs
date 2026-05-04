using FluentAssertions;
using TrucType = ZenTam.Api.Features.Calendars.Models.TrucType;

namespace ZenTam.Api.UnitTests.CanChi.Nomenclature;

/// <summary>
/// Contract tests to verify TrucType enum naming follows Ngọc Hạp Thông Thư nomenclature.
/// All 12 values must use "Truc" prefix: TrucKien, TrucTru, TrucMan, TrucBinh,
/// TrucDinh, TrucChap, TrucPha, TrucNguy, TrucThanh, TrucThu, TrucKhai, TrucBe.
/// 
/// These tests verify the NEW nomenclature, NOT the old abbreviated names.
/// </summary>
public class TrucTypeNomenclatureTests
{
    [Fact]
    public void TrucType_Has12Values()
    {
        // Verify we have exactly 12 Truc values (enum count)
        var count = Enum.GetValues<TrucType>().Length;
        count.Should().Be(12);
    }

    [Fact]
    public void TrucType_TrucKien_IsZero()
    {
        // First value must be TrucKien = 0
        var value = (int)TrucType.TrucKien;
        value.Should().Be(0);
    }

    [Fact]
    public void TrucType_TrucBe_IsEleven()
    {
        // Last value must be TrucBe = 11
        var value = (int)TrucType.TrucBe;
        value.Should().Be(11);
    }

    [Fact]
    public void TrucType_All12ValuesSequential()
    {
        // Verify all 12 values are sequential 0-11
        var values = Enum.GetValues<TrucType>();
        values.Should().HaveCount(12);
        
        for (int i = 0; i < 12; i++)
        {
            ((int)values[i]).Should().Be(i);
        }
    }

    [Fact]
    public void TrucType_AllNamesHaveTrucPrefix()
    {
        // Verify all enum names start with "Truc"
        var names = Enum.GetNames<TrucType>();
        
        foreach (var name in names)
        {
            name.Should().StartWith("Truc", 
                $"Enum value '{name}' should start with 'Truc' prefix per Ngọc Hạp Thông Thư");
        }
    }

    [Fact]
    public void TrucType_ContainsAllRequiredNames()
    {
        // Verify all 12 required names are present with correct casing
        var type = typeof(TrucType);
        
        type.GetField("TrucKien").Should().NotBeNull();
        type.GetField("TrucTru").Should().NotBeNull();
        type.GetField("TrucMan").Should().NotBeNull();
        type.GetField("TrucBinh").Should().NotBeNull();
        type.GetField("TrucDinh").Should().NotBeNull();
        type.GetField("TrucChap").Should().NotBeNull();
        type.GetField("TrucPha").Should().NotBeNull();
        type.GetField("TrucNguy").Should().NotBeNull();
        type.GetField("TrucThanh").Should().NotBeNull();
        type.GetField("TrucThu").Should().NotBeNull();
        type.GetField("TrucKhai").Should().NotBeNull();
        type.GetField("TrucBe").Should().NotBeNull();
    }

    [Fact]
    public void TrucType_OldNamesDoNotExist()
    {
        // Verify old abbreviated names (without Truc prefix) do NOT exist
        var type = typeof(TrucType);
        
        // These old names should NOT exist
        type.GetField("Kiến").Should().BeNull();
        type.GetField("Trừ").Should().BeNull();
        type.GetField("Mãn").Should().BeNull();
        type.GetField("Bình").Should().BeNull();
        type.GetField("Định").Should().BeNull();
        type.GetField("Chấp").Should().BeNull();
        type.GetField("Phá").Should().BeNull();
        type.GetField("Nguy").Should().BeNull();
        type.GetField("Thành").Should().BeNull();
        type.GetField("Thu").Should().BeNull();
        type.GetField("Khai").Should().BeNull();
        type.GetField("Bế").Should().BeNull();
    }
}