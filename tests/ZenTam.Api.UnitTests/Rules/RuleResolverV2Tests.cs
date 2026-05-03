using FluentAssertions;
using ZenTam.Api.Common.CanChi.Models;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Rules;
using ZenTam.Api.Common.Rules.Models;
using ZenTam.Api.Features.EvaluateSpiritualAction.Rules;

namespace ZenTam.Api.UnitTests.Rules;

public class RuleResolverV2Tests
{
    private static UserProfile CreateProfile(
        Gender gender = Gender.Male,
        int lunarYOB = 1990,
        int targetYear = 2025)
    {
        return new UserProfile
        {
            Gender = gender,
            LunarYOB = lunarYOB,
            TargetYear = targetYear
        };
    }

    private static LunarDateContext CreateLunarContext(
        int lunarYear = 2025,
        int lunarMonth = 1,
        int lunarDay = 1,
        bool isLeap = false,
        int jdn = 2460635)
    {
        return new LunarDateContext
        {
            LunarYear = lunarYear,
            LunarMonth = lunarMonth,
            LunarDay = lunarDay,
            IsLeap = isLeap,
            Jdn = jdn
        };
    }

    private static RuleContext CreateContext(
        UserProfile? profile = null,
        LunarDateContext? lunar = null,
        CanChiDay? canChiNgay = null,
        CanChiYear? canChiTuoi = null)
    {
        return new RuleContext
        {
            Profile = profile ?? CreateProfile(),
            Lunar = lunar ?? CreateLunarContext(),
            CanChiNgay = canChiNgay,
            CanChiTuoi = canChiTuoi
        };
    }

    [Fact]
    public void Constructor_RegistersRulesByRuleCode()
    {
        // Arrange
        var rules = new ISpiritualRule[]
        {
            new NguyetKyRuleV2(),
            new TamNuongRuleV2(),
            new SatChuRuleV2(),
            new XungTuoiRuleV2()
        };

        // Act
        var resolver = new RuleResolverV2(rules);

        // Assert
        var all = resolver.All;
        all.Should().HaveCount(4);
        all.Should().ContainKey("NGUYET_KY");
        all.Should().ContainKey("TAM_NUONG");
        all.Should().ContainKey("SAT_CHU");
        all.Should().ContainKey("XUNG_TUOI");
    }

    [Fact]
    public void Get_ExistingRuleCode_ReturnsRule()
    {
        // Arrange
        var rules = new ISpiritualRule[]
        {
            new NguyetKyRuleV2(),
            new TamNuongRuleV2()
        };
        var resolver = new RuleResolverV2(rules);

        // Act
        var rule = resolver.Get("NGUYET_KY");

        // Assert
        rule.Should().NotBeNull();
        rule.Should().BeOfType<NguyetKyRuleV2>();
    }

    [Fact]
    public void Get_UnknownRuleCode_ReturnsNull()
    {
        // Arrange
        var rules = new ISpiritualRule[]
        {
            new NguyetKyRuleV2()
        };
        var resolver = new RuleResolverV2(rules);

        // Act
        var rule = resolver.Get("UNKNOWN_RULE");

        // Assert
        rule.Should().BeNull();
    }

    [Fact]
    public void All_ReturnsFullRegistryDictionary()
    {
        // Arrange
        var rules = new ISpiritualRule[]
        {
            new NguyetKyRuleV2(),
            new TamNuongRuleV2(),
            new SatChuRuleV2(),
            new XungTuoiRuleV2()
        };
        var resolver = new RuleResolverV2(rules);

        // Act
        var all = resolver.All;

        // Assert
        all.Should().HaveCount(4);
        all.Should().BeAssignableTo<IReadOnlyDictionary<string, ISpiritualRule>>();
    }

    [Fact]
    public void All_ReturnsReadOnlyDictionary()
    {
        // Arrange
        var rules = new ISpiritualRule[]
        {
            new NguyetKyRuleV2()
        };
        var resolver = new RuleResolverV2(rules);

        // Act
        var all = resolver.All;

        // Assert - the returned dictionary is IReadOnlyDictionary
        all.Should().BeAssignableTo<IReadOnlyDictionary<string, ISpiritualRule>>();
        all.Should().HaveCount(1);
    }

    [Fact]
    public void Get_MultipleRules_ReturnsCorrectRuleForEachCode()
    {
        // Arrange
        var rules = new ISpiritualRule[]
        {
            new NguyetKyRuleV2(),
            new TamNuongRuleV2(),
            new SatChuRuleV2(),
            new XungTuoiRuleV2()
        };
        var resolver = new RuleResolverV2(rules);

        // Act & Assert
        resolver.Get("NGUYET_KY").Should().BeOfType<NguyetKyRuleV2>();
        resolver.Get("TAM_NUONG").Should().BeOfType<TamNuongRuleV2>();
        resolver.Get("SAT_CHU").Should().BeOfType<SatChuRuleV2>();
        resolver.Get("XUNG_TUOI").Should().BeOfType<XungTuoiRuleV2>();
    }

    [Fact]
    public void Constructor_WithEmptyRules_RegistryIsEmpty()
    {
        // Arrange
        var rules = Array.Empty<ISpiritualRule>();

        // Act
        var resolver = new RuleResolverV2(rules);

        // Assert
        resolver.All.Should().BeEmpty();
    }

    [Fact]
    public void Get_WithEmptyRegistry_ReturnsNull()
    {
        // Arrange
        var rules = Array.Empty<ISpiritualRule>();
        var resolver = new RuleResolverV2(rules);

        // Act
        var rule = resolver.Get("NGUYET_KY");

        // Assert
        rule.Should().BeNull();
    }

    [Fact]
    public void StandardTestFixture_ResolverWorksWithAllV2Rules()
    {
        // This test validates the standard test fixtures work with RuleResolverV2.

        // Arrange - Standard fixture from user request
        var profile = new UserProfile
        {
            Gender = Gender.Male,
            LunarYOB = 1990,
            TargetYear = 2025
        };

        var lunar = new LunarDateContext
        {
            LunarYear = 2025,
            LunarMonth = 1,
            LunarDay = 1,
            IsLeap = false,
            Jdn = 2460635
        };

        var context = new RuleContext
        {
            Profile = profile,
            Lunar = lunar
        };

        var rules = new ISpiritualRule[]
        {
            new NguyetKyRuleV2(),
            new TamNuongRuleV2(),
            new SatChuRuleV2(),
            new XungTuoiRuleV2()
        };

        var resolver = new RuleResolverV2(rules);

        // Act - get each rule and evaluate
        var nguyetKy = resolver.Get("NGUYET_KY")!.Evaluate(context);
        var tamNuong = resolver.Get("TAM_NUONG")!.Evaluate(context);
        var satChu = resolver.Get("SAT_CHU")!.Evaluate(context);
        var xungTuoi = resolver.Get("XUNG_TUOI")!.Evaluate(context);

        // Assert - all rules should evaluate without throwing
        nguyetKy.RuleCode.Should().Be("NGUYET_KY");
        tamNuong.RuleCode.Should().Be("TAM_NUONG");
        satChu.RuleCode.Should().Be("SAT_CHU");
        xungTuoi.RuleCode.Should().Be("XUNG_TUOI");
    }
}
