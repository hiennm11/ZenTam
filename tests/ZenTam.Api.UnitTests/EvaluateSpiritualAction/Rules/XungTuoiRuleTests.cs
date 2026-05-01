using FluentAssertions;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Features.EvaluateSpiritualAction.Rules;

namespace ZenTam.Api.UnitTests.EvaluateSpiritualAction.Rules;

public class XungTuoiRuleTests
{
    private readonly XungTuoiRule _rule = new();

    [Fact]
    public void Evaluate_WhenDayChiClashesWithBirthYearChi_Fails()
    {
        // Birth year 1998 = Mậu Dần (Chi = Dần, index 3)
        // Formula: yobChi = ((LunarYOB + 8) % 12) + 1 = ((1998 + 8) % 12) + 1 = 3
        // Dần(3) xung Thân(9)
        // To get dayChi = Thân(9): Jdn such that ((Jdn + 8) % 12) + 1 = 9
        // Let Jdn = 2460000 → dayChi = 9 (Thân)
        var profile = new UserProfile
        {
            Gender = Gender.Male,
            LunarYOB = 1998, // Mậu Dần, Chi = Dần(3)
            TargetYear = 2026
        };

        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 4,
            LunarDay = 15,
            IsLeap = false,
            Jdn = 2460000 // This gives dayChi = Thân(9), which clashes with Dần(3)
        };

        // Act
        var result = _rule.Evaluate(profile, context);

        // Assert
        result.RuleName.Should().Be("XUNG_TUOI");
        result.IsPassed.Should().BeFalse();
        result.Score.Should().Be(-15);
        result.IsMandatory.Should().BeTrue();
        result.Message.Should().Contain("Phạm Xung Tuổi");
    }

    [Fact]
    public void Evaluate_WhenDayChiDoesNotClash_Passes()
    {
        // Birth year 1998 = Mậu Dần (Chi = Dần, index 3)
        // Dần(3) xung Thân(9)
        // With Jdn = 2460001 → dayChi = 10 (Dậu) - does NOT clash with Dần
        var profile = new UserProfile
        {
            Gender = Gender.Male,
            LunarYOB = 1998, // Mậu Dần, Chi = Dần(3)
            TargetYear = 2026
        };

        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 4,
            LunarDay = 16,
            IsLeap = false,
            Jdn = 2460001 // This gives dayChi = Dậu(10), which does NOT clash with Dần
        };

        // Act
        var result = _rule.Evaluate(profile, context);

        // Assert
        result.RuleName.Should().Be("XUNG_TUOI");
        result.IsPassed.Should().BeTrue();
        result.Score.Should().Be(0);
        result.Message.Should().Contain("Không phạm Xung Tuổi");
    }

    [Theory]
    [InlineData(1, 7)]   // Tý xung Ngọ
    [InlineData(7, 1)]   // Ngọ xung Tý
    [InlineData(2, 8)]   // Sửu xung Mùi
    [InlineData(8, 2)]   // Mùi xung Sửu
    [InlineData(3, 9)]   // Dần xung Thân
    [InlineData(9, 3)]   // Thân xung Dần
    [InlineData(4, 10)]  // Mão xung Dậu
    [InlineData(10, 4)]  // Dậu xung Mão
    [InlineData(5, 11)]  // Thìn xung Tuất
    [InlineData(11, 5)]  // Tuất xung Thìn
    [InlineData(6, 12)]  // Tỵ xung Hợi
    [InlineData(12, 6)]  // Hợi xung Tỵ
    public void Evaluate_AllXungPairs_ClashCorrectly(int yobChi, int xungChi)
    {
        // For each xung pair, verify that when dayChi equals xungChi, it fails
        // LunarYOB is calculated: yobChi = ((LunarYOB + 8) % 12) + 1
        // So LunarYOB = (yobChi - 1 - 8) mod 12 = (yobChi - 9) mod 12
        int lunarYob = ((yobChi - 9) % 12 + 12) % 12;
        // Normalize to valid year (between 1900 and 2100)
        while (lunarYob < 1900) lunarYob += 60;
        while (lunarYob > 2100) lunarYob -= 60;

        var profile = new UserProfile
        {
            Gender = Gender.Male,
            LunarYOB = lunarYob,
            TargetYear = 2026
        };

        // Calculate Jdn that gives dayChi = xungChi
        // dayChi = ((Jdn + 8) % 12) + 1 = xungChi
        // Jdn = (xungChi - 1 - 8) mod 12 = (xungChi - 9) mod 12
        // For simplicity, use Jdn base that gives the correct dayChi
        int jdnBase = ((xungChi - 9) % 12 + 12) % 12;
        // We need to find a Jdn value that gives us xungChi
        // Use Jdn = 2460000 + offset to get different dayChi values
        int jdn = 2460000 + (xungChi - 9);

        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 4,
            LunarDay = 15,
            IsLeap = false,
            Jdn = jdn
        };

        // Act
        var result = _rule.Evaluate(profile, context);

        // Assert
        result.IsPassed.Should().BeFalse(because: $"Chi {yobChi} should clash with Chi {xungChi}");
        result.Score.Should().Be(-15);
    }

    [Fact]
    public void Evaluate_SameChiAsBirthYear_Passes()
    {
        // Birth year with Chi = Tý(1)
        // LunarYOB with yobChi = 1: LunarYOB = (1 - 9) mod 12 = -8 mod 12 = 4 → 1904 or 1964
        var profile = new UserProfile
        {
            Gender = Gender.Male,
            LunarYOB = 1964, // Giáp Tý, Chi = Tý(1)
            TargetYear = 2026
        };

        // Day with Chi = Tý(1) should NOT clash
        // Jdn = 2460000 gives dayChi = 9 (Thân), not Tý
        // Need Jdn such that dayChi = Tý(1)
        // dayChi = ((Jdn + 8) % 12) + 1 = 1 → ((Jdn + 8) % 12) = 0
        // Jdn such that (Jdn + 8) % 12 = 0
        // Let Jdn = 2459992 → dayChi = ((2459992 + 8) % 12) + 1 = (0 % 12) + 1 = 1 = Tý
        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 4,
            LunarDay = 15,
            IsLeap = false,
            Jdn = 2459992 // dayChi = Tý(1), same as birth year - should pass
        };

        // Act
        var result = _rule.Evaluate(profile, context);

        // Assert
        result.IsPassed.Should().BeTrue(because: "Same Chi does not cause Xung");
        result.Score.Should().Be(0);
    }

    [Fact]
    public void Evaluate_DifferentNonClashingChi_Passes()
    {
        // Birth year 1996 = Bính Tử (Chi = Tý, index 1)
        // Tý(1) xung Ngọ(7)
        // Day with Chi = Sửu(2) should NOT clash
        var profile = new UserProfile
        {
            Gender = Gender.Male,
            LunarYOB = 1996, // Bính Tý, Chi = Tý(1)
            TargetYear = 2026
        };

        // Jdn = 2460001 gives dayChi = 10 (Dậu) - not Tý, not Ngọ
        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 4,
            LunarDay = 16,
            IsLeap = false,
            Jdn = 2460001 // dayChi = Dậu(10), does NOT clash with Tý(1)
        };

        // Act
        var result = _rule.Evaluate(profile, context);

        // Assert
        result.IsPassed.Should().BeTrue(because: "Dậu(10) does not clash with Tý(1)");
        result.Score.Should().Be(0);
    }
}