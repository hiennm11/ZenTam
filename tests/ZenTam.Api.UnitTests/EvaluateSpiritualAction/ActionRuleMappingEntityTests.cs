using FluentAssertions;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.UnitTests.EvaluateSpiritualAction;

/// <summary>
/// Unit tests for ActionRuleMapping entity construction and enum values.
/// Verifies entity defaults and enum value correctness per contract test matrix.
/// </summary>
public class ActionRuleMappingEntityTests
{
    // ========== Entity Construction Tests ==========

    [Fact]
    public void DefaultConstruction_HasCorrectDefaults()
    {
        // Act
        var mapping = new ActionRuleMapping();

        // Assert - per contract: Default GenderScope=Both, Tier=All, Priority=0
        mapping.GenderScope.Should().Be(GenderApplyScope.Both);
        mapping.Tier.Should().Be(RuleTier.All);
        mapping.Priority.Should().Be(0);
    }

    [Fact]
    public void ConstructWithAllFields_AllPropertiesSetCorrectly()
    {
        // Arrange & Act
        var mapping = new ActionRuleMapping
        {
            Id = 1,
            ActionId = "XAY_NHA",
            RuleCode = "KimLau",
            IsMandatory = true,
            GenderScope = GenderApplyScope.MaleOnly,
            Tier = RuleTier.Year,
            Priority = 1
        };

        // Assert
        mapping.Id.Should().Be(1);
        mapping.ActionId.Should().Be("XAY_NHA");
        mapping.RuleCode.Should().Be("KimLau");
        mapping.IsMandatory.Should().BeTrue();
        mapping.GenderScope.Should().Be(GenderApplyScope.MaleOnly);
        mapping.Tier.Should().Be(RuleTier.Year);
        mapping.Priority.Should().Be(1);
    }

    [Fact]
    public void DefaultConstruction_GenderScopeDefaultsToBoth()
    {
        // Act
        var mapping = new ActionRuleMapping();

        // Assert - per contract: Defaults to GenderApplyScope.Both (value 0)
        mapping.GenderScope.Should().Be(GenderApplyScope.Both);
        mapping.GenderScope.Should().Be((GenderApplyScope)0);
    }

    [Fact]
    public void DefaultConstruction_TierDefaultsToAll()
    {
        // Act
        var mapping = new ActionRuleMapping();

        // Assert - per contract: Defaults to RuleTier.All (value 3)
        mapping.Tier.Should().Be(RuleTier.All);
        mapping.Tier.Should().Be((RuleTier)3);
    }

    [Fact]
    public void DefaultConstruction_PriorityDefaultsToZero()
    {
        // Act
        var mapping = new ActionRuleMapping();

        // Assert - per contract: Priority=0 by default
        mapping.Priority.Should().Be(0);
    }

    // ========== GenderApplyScope Enum Values ==========

    [Theory]
    [InlineData(GenderApplyScope.Both, 0)]
    [InlineData(GenderApplyScope.MaleOnly, 1)]
    [InlineData(GenderApplyScope.FemaleOnly, 2)]
    public void GenderApplyScope_EnumValues_AreCorrect(GenderApplyScope scope, int expectedValue)
    {
        // Assert - per contract: Both=0, MaleOnly=1, FemaleOnly=2
        ((int)scope).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(GenderApplyScope.Both)]
    [InlineData(GenderApplyScope.MaleOnly)]
    [InlineData(GenderApplyScope.FemaleOnly)]
    public void GenderApplyScope_AllValues_AreDefined(GenderApplyScope scope)
    {
        // Arrange
        var enumValues = Enum.GetValues<GenderApplyScope>();

        // Assert - all scope values should be valid enum members
        enumValues.Should().Contain(scope);
    }

    // ========== RuleTier Enum Values ==========

    [Theory]
    [InlineData(RuleTier.Year, 0)]
    [InlineData(RuleTier.Month, 1)]
    [InlineData(RuleTier.Day, 2)]
    [InlineData(RuleTier.All, 3)]
    public void RuleTier_EnumValues_AreCorrect(RuleTier tier, int expectedValue)
    {
        // Assert - per contract: Year=0, Month=1, Day=2, All=3
        ((int)tier).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(RuleTier.Year)]
    [InlineData(RuleTier.Month)]
    [InlineData(RuleTier.Day)]
    [InlineData(RuleTier.All)]
    public void RuleTier_AllValues_AreDefined(RuleTier tier)
    {
        // Arrange
        var enumValues = Enum.GetValues<RuleTier>();

        // Assert - all tier values should be valid enum members
        enumValues.Should().Contain(tier);
    }

    // ========== Entity Property Validation ==========

    [Theory]
    [InlineData(GenderApplyScope.Both)]
    [InlineData(GenderApplyScope.MaleOnly)]
    [InlineData(GenderApplyScope.FemaleOnly)]
    public void GenderScope_ValidValues_Accepted(GenderApplyScope scope)
    {
        // Arrange
        var mapping = new ActionRuleMapping { GenderScope = scope };

        // Assert
        mapping.GenderScope.Should().Be(scope);
    }

    [Theory]
    [InlineData(RuleTier.Year)]
    [InlineData(RuleTier.Month)]
    [InlineData(RuleTier.Day)]
    [InlineData(RuleTier.All)]
    public void Tier_ValidValues_Accepted(RuleTier tier)
    {
        // Arrange
        var mapping = new ActionRuleMapping { Tier = tier };

        // Assert
        mapping.Tier.Should().Be(tier);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Priority_ValidRange_Accepted(int priority)
    {
        // Arrange
        var mapping = new ActionRuleMapping { Priority = priority };

        // Assert
        mapping.Priority.Should().Be(priority);
    }

    [Fact]
    public void Priority_Zero_IsValid()
    {
        // Arrange
        var mapping = new ActionRuleMapping { Priority = 0 };

        // Assert
        mapping.Priority.Should().Be(0);
    }

    [Fact]
    public void IsMandatory_CanBeTrue()
    {
        // Arrange
        var mapping = new ActionRuleMapping { IsMandatory = true };

        // Assert
        mapping.IsMandatory.Should().BeTrue();
    }

    [Fact]
    public void IsMandatory_CanBeFalse()
    {
        // Arrange
        var mapping = new ActionRuleMapping { IsMandatory = false };

        // Assert
        mapping.IsMandatory.Should().BeFalse();
    }
}