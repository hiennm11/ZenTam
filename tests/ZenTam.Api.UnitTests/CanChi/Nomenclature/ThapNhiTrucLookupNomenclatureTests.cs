namespace ZenTam.Api.UnitTests.CanChi.Nomenclature;

using FluentAssertions;

/// <summary>
/// Contract tests to verify ThapNhiTrucLookup display names follow Ngọc Hạp Thông Thư nomenclature.
/// All 12 names must use "Trực X" format: "Trực Kiến", "Trực Trừ", "Trực Mãn", etc.
/// 
/// These tests verify the NEW nomenclature, NOT the old abbreviated names.
/// </summary>
public class ThapNhiTrucLookupNomenclatureTests
{
    private static readonly string[] ExpectedNewNames =
    [
        "Trực Kiến", "Trực Trừ", "Trực Mãn", "Trực Bình", "Trực Định", "Trực Chấp",
        "Trực Phá", "Trực Nguy", "Trực Thành", "Trực Thu", "Trực Khai", "Trực Bế"
    ];

    private static readonly string[] OldAbbreviatedNames =
    [
        "Kiến", "Trừ", "Mãn", "Bình", "Định", "Chấp", "Phá", "Nguy", "Thành", "Thu", "Khai", "Bế"
    ];

    [Theory]
    [InlineData(0, "Trực Kiến")]
    [InlineData(1, "Trực Trừ")]
    [InlineData(2, "Trực Mãn")]
    [InlineData(3, "Trực Bình")]
    [InlineData(4, "Trực Định")]
    [InlineData(5, "Trực Chấp")]
    [InlineData(6, "Trực Phá")]
    [InlineData(7, "Trực Nguy")]
    [InlineData(8, "Trực Thành")]
    [InlineData(9, "Trực Thu")]
    [InlineData(10, "Trực Khai")]
    [InlineData(11, "Trực Bế")]
    public void GetTrucName_ReturnsNewFullName(int index, string expectedName)
    {
        var result = ZenTam.Api.Features.Calendars.Data.ThapNhiTrucLookup.GetTrucName(index);
        result.Should().Be(expectedName);
    }

    [Fact]
    public void GetTrucName_Index0_ReturnsTrựcKiến()
    {
        var result = ZenTam.Api.Features.Calendars.Data.ThapNhiTrucLookup.GetTrucName(0);
        result.Should().Be("Trực Kiến");
    }

    [Fact]
    public void GetTrucName_Index1_ReturnsTrựcTrừ()
    {
        var result = ZenTam.Api.Features.Calendars.Data.ThapNhiTrucLookup.GetTrucName(1);
        result.Should().Be("Trực Trừ");
    }

    [Fact]
    public void GetTrucName_Index11_ReturnsTrựcBế()
    {
        var result = ZenTam.Api.Features.Calendars.Data.ThapNhiTrucLookup.GetTrucName(11);
        result.Should().Be("Trực Bế");
    }

    [Fact]
    public void GetTrucName_All12Names_StartWithTrực()
    {
        for (int i = 0; i < 12; i++)
        {
            var name = ZenTam.Api.Features.Calendars.Data.ThapNhiTrucLookup.GetTrucName(i);
            name.Should().StartWith("Trực", 
                $"Name at index {i} should start with 'Trực' prefix per Ngọc Hạp Thông Thư");
        }
    }

    [Fact]
    public void GetTrucName_All12Names_MatchExpectedNewFormat()
    {
        for (int i = 0; i < 12; i++)
        {
            var result = ZenTam.Api.Features.Calendars.Data.ThapNhiTrucLookup.GetTrucName(i);
            result.Should().Be(ExpectedNewNames[i], 
                $"Index {i} should return '{ExpectedNewNames[i]}' per Ngọc Hạp Thông Thư");
        }
    }

    [Fact]
    public void GetTrucName_DoesNotReturnOldAbbreviatedNames()
    {
        // Verify old abbreviated names are NOT returned
        for (int i = 0; i < 12; i++)
        {
            var result = ZenTam.Api.Features.Calendars.Data.ThapNhiTrucLookup.GetTrucName(i);
            result.Should().NotBe(OldAbbreviatedNames[i],
                $"Index {i} should NOT return old abbreviated name '{OldAbbreviatedNames[i]}'");
        }
    }

    [Fact]
    public void GetTrucName_Index0_DoesNotReturnOldKiến()
    {
        var result = ZenTam.Api.Features.Calendars.Data.ThapNhiTrucLookup.GetTrucName(0);
        result.Should().NotBe("Kiến", "Old abbreviated name should not be used");
    }

    [Fact]
    public void GetTrucName_Index1_DoesNotReturnOldTrừ()
    {
        var result = ZenTam.Api.Features.Calendars.Data.ThapNhiTrucLookup.GetTrucName(1);
        result.Should().NotBe("Trừ", "Old abbreviated name should not be used");
    }

    [Fact]
    public void GetTrucName_IndexNeg1_ThrowsArgumentOutOfRangeException()
    {
        var act = () => ZenTam.Api.Features.Calendars.Data.ThapNhiTrucLookup.GetTrucName(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetTrucName_Index12_ThrowsArgumentOutOfRangeException()
    {
        var act = () => ZenTam.Api.Features.Calendars.Data.ThapNhiTrucLookup.GetTrucName(12);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetTrucIndex_TyMonth1_Returns0()
    {
        // Tý (chi=0), Month 1 → index 0 (Trực Kiến)
        var result = ZenTam.Api.Features.Calendars.Data.ThapNhiTrucLookup.GetTrucIndex(0, 1);
        result.Should().Be(0);
    }

    [Fact]
    public void GetTrucIndex_TyMonth12_Returns11()
    {
        // Tý (chi=0), Month 12 → index 11 (Trực Bế)
        var result = ZenTam.Api.Features.Calendars.Data.ThapNhiTrucLookup.GetTrucIndex(0, 12);
        result.Should().Be(11);
    }
}