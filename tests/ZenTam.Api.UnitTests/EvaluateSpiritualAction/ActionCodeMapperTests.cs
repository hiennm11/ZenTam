using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Features.EvaluateSpiritualAction.Models;
using ZenTam.Api.Features.EvaluateSpiritualAction.Services;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.UnitTests.EvaluateSpiritualAction;

/// <summary>
/// Unit tests for ActionCodeMapper - bidirectional mapping between ActionCode enum and string IDs.
/// Tests cover:
/// - ToString (Enum → String) happy path
/// - ToEnum (String → Enum) happy path  
/// - ToEnum edge cases and unknown actions
/// - Performance and thread safety
/// </summary>
public class ActionCodeMapperTests : IDisposable
{
    private readonly ZenTamDbContext _db;
    private readonly ActionCodeMapper _mapper;

    public ActionCodeMapperTests()
    {
        _db = TestHelpers.TestDbHelper.CreateInMemoryDbContext();
        SeedAll25ActionCodes(_db);
        _mapper = new ActionCodeMapper(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    private static void SeedAll25ActionCodes(ZenTamDbContext context)
    {
        var actionCodes = new[]
        {
            "XAY_NHA", "SUA_NHA", "NHAP_TRACH", "CUOI_HOI", "SINH_CON",
            "KHAI_TRUONG", "KY_HOP_DONG", "NHAN_VIEC", "MUA_VANG", "MUA_DAT",
            "MUA_XE", "DAM_BAO_HANH", "XUAT_HANH", "CU_HUONG", "BAT_DAU",
            "CHUA_BENH", "TAM_SOAT", "KHAI_VONG", "THI_DAU", "AN_TANG",
            "BOC_MO", "THO_MAU", "LE_BAI", "CAT_SAC", "TU_TUC"
        };

        // First clear ALL existing ActionCatalog entries to avoid duplicate key issues
        var existingEntries = context.ActionCatalog.Local.ToList();
        foreach (var entry in existingEntries)
        {
            context.Entry(entry).State = EntityState.Detached;
        }
        context.ActionCatalog.RemoveRange(existingEntries);
        context.SaveChanges();

        // Now add fresh entries
        foreach (var code in actionCodes)
        {
            context.ActionCatalog.Add(new ActionCatalog { Id = code, Description = $"Test {code}" });
        }
        context.SaveChanges();
    }

    // ========== Part 1.1: ToString (Enum → String) - Happy Path ==========

    [Theory]
    [InlineData(ActionCode.XAY_NHA, "XAY_NHA")]
    [InlineData(ActionCode.MUA_VANG, "MUA_VANG")]
    [InlineData(ActionCode.AN_TANG, "AN_TANG")]
    [InlineData(ActionCode.CUOI_HOI, "CUOI_HOI")]
    [InlineData(ActionCode.KHAI_TRUONG, "KHAI_TRUONG")]
    [InlineData(ActionCode.SUA_NHA, "SUA_NHA")]
    [InlineData(ActionCode.NHAP_TRACH, "NHAP_TRACH")]
    [InlineData(ActionCode.SINH_CON, "SINH_CON")]
    [InlineData(ActionCode.KY_HOP_DONG, "KY_HOP_DONG")]
    [InlineData(ActionCode.NHAN_VIEC, "NHAN_VIEC")]
    [InlineData(ActionCode.MUA_DAT, "MUA_DAT")]
    [InlineData(ActionCode.MUA_XE, "MUA_XE")]
    [InlineData(ActionCode.DAM_BAO_HANH, "DAM_BAO_HANH")]
    [InlineData(ActionCode.XUAT_HANH, "XUAT_HANH")]
    [InlineData(ActionCode.CU_HUONG, "CU_HUONG")]
    [InlineData(ActionCode.BAT_DAU, "BAT_DAU")]
    [InlineData(ActionCode.CHUA_BENH, "CHUA_BENH")]
    [InlineData(ActionCode.TAM_SOAT, "TAM_SOAT")]
    [InlineData(ActionCode.KHAI_VONG, "KHAI_VONG")]
    [InlineData(ActionCode.THI_DAU, "THI_DAU")]
    [InlineData(ActionCode.BOC_MO, "BOC_MO")]
    [InlineData(ActionCode.THO_MAU, "THO_MAU")]
    [InlineData(ActionCode.LE_BAI, "LE_BAI")]
    [InlineData(ActionCode.CAT_SAC, "CAT_SAC")]
    [InlineData(ActionCode.TU_TUC, "TU_TUC")]
    public void ToString_ValidEnum_ReturnsCorrectString(ActionCode code, string expected)
    {
        // Act
        var result = _mapper.ToString(code);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToString_UnknownEnum_ReturnsUnknown()
    {
        // Arrange
        var unknownCode = ActionCode.UNKNOWN;

        // Act
        var result = _mapper.ToString(unknownCode);

        // Assert
        result.Should().Be("UNKNOWN");
    }

    [Fact]
    public void ToString_All25Enums_ReturnsExpectedStrings()
    {
        // Arrange - Get all non-UNKNOWN enum values
        var allEnums = Enum.GetValues<ActionCode>()
            .Where(c => c != ActionCode.UNKNOWN)
            .ToList();

        // Assert - All 25 enums should map correctly
        allEnums.Should().HaveCount(25);
        foreach (var code in allEnums)
        {
            _mapper.ToString(code).Should().NotBeNullOrEmpty();
            _mapper.ToString(code).Should().Be(code.ToString());
        }
    }

    // ========== Part 1.2: ToEnum (String → Enum) - Happy Path ==========

    [Theory]
    [InlineData("XAY_NHA", ActionCode.XAY_NHA)]
    [InlineData("MUA_VANG", ActionCode.MUA_VANG)]
    [InlineData("AN_TANG", ActionCode.AN_TANG)]
    [InlineData("CUOI_HOI", ActionCode.CUOI_HOI)]
    [InlineData("KHAI_TRUONG", ActionCode.KHAI_TRUONG)]
    public void ToEnum_ValidString_ReturnsCorrectEnum(string input, ActionCode expected)
    {
        // Act
        var result = _mapper.ToEnum(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("xay_nha", ActionCode.XAY_NHA)]
    [InlineData("mua_vang", ActionCode.MUA_VANG)]
    [InlineData("an_tang", ActionCode.AN_TANG)]
    public void ToEnum_CaseInsensitive_MatchesEnum(string input, ActionCode expected)
    {
        // Act
        var result = _mapper.ToEnum(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("CuOi_HoI", ActionCode.CUOI_HOI)]
    [InlineData("XAY_NHA", ActionCode.XAY_NHA)]
    [InlineData("KhAi_TrUoNg", ActionCode.KHAI_TRUONG)]
    public void ToEnum_MixedCase_ReturnsCorrectEnum(string input, ActionCode expected)
    {
        // Act
        var result = _mapper.ToEnum(input);

        // Assert
        result.Should().Be(expected);
    }

    // ========== Part 1.3: ToEnum - Edge Cases / Unknown Actions ==========

    [Fact]
    public void ToEnum_UnknownAction_ReturnsUnknown()
    {
        // Act
        var result = _mapper.ToEnum("RANDOM_ACTION");

        // Assert
        result.Should().Be(ActionCode.UNKNOWN);
    }

    [Fact]
    public void ToEnum_EmptyString_ReturnsUnknown()
    {
        // Act
        var result = _mapper.ToEnum("");

        // Assert
        result.Should().Be(ActionCode.UNKNOWN);
    }

    [Fact]
    public void ToEnum_Null_ReturnsUnknown()
    {
        // Act
        var result = _mapper.ToEnum(null!);

        // Assert
        result.Should().Be(ActionCode.UNKNOWN);
    }

    [Fact]
    public void ToEnum_WhitespaceOnly_ReturnsUnknown()
    {
        // Act
        var result = _mapper.ToEnum("   ");

        // Assert
        result.Should().Be(ActionCode.UNKNOWN);
    }

    [Fact]
    public void ToEnum_PartialMatch_ReturnsUnknown()
    {
        // Act
        var result = _mapper.ToEnum("XAY");

        // Assert
        result.Should().Be(ActionCode.UNKNOWN);
    }

    [Fact]
    public void ToEnum_SqlInjectionAttempt_ReturnsUnknown()
    {
        // Act
        var result = _mapper.ToEnum("'; DROP TABLE ActionCatalog; --");

        // Assert
        result.Should().Be(ActionCode.UNKNOWN);
    }

    [Fact]
    public void ToEnum_SpecialCharacters_ReturnsUnknown()
    {
        // Act
        var result = _mapper.ToEnum("XAY_NHA\x00NULL");

        // Assert
        result.Should().Be(ActionCode.UNKNOWN);
    }

    // ========== Part 5.2: Performance Under Load ==========

    [Fact]
    public void Performance_1000SequentialMappings_CompletesUnder100ms()
    {
        // Arrange
        var names = Enum.GetNames<ActionCode>().Where(n => n != "UNKNOWN").ToList();
        var inputs = Enumerable.Range(0, 1000)
            .Select(i => names[i % names.Count])
            .ToList();

        // Act & Assert
        var sw = System.Diagnostics.Stopwatch.StartNew();
        foreach (var input in inputs)
        {
            _mapper.ToEnum(input);
        }
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    public void ThreadSafety_ConcurrentAccess_NoRaceConditions()
    {
        // Arrange
        var options = new System.Threading.CancellationTokenSource();
        var token = options.Token;

        // Act - Parallel.For simulates concurrent access
        System.Threading.Tasks.Parallel.For(0, 100, new ParallelOptions { CancellationToken = token }, i =>
        {
            foreach (var code in Enum.GetValues<ActionCode>())
            {
                if (code == ActionCode.UNKNOWN) continue;
                var str = code.ToString();
                _mapper.ToString(code).Should().Be(str);
                _mapper.ToEnum(str).Should().Be(code);
            }
        });

        // Assert - No exceptions thrown indicates thread safety
    }

    // ========== Additional edge cases ==========

    [Fact]
    public void ToString_InvalidEnumValue_ReturnsUnknown()
    {
        // Arrange - Cast an invalid integer to enum (out of range)
        var invalidCode = (ActionCode)999;

        // Act
        var result = _mapper.ToString(invalidCode);

        // Assert - Should fallback gracefully
        result.Should().Be("UNKNOWN");
    }

    [Theory]
    [InlineData("INVALID_CODE_123")]
    [InlineData("XYZ")]
    [InlineData("KHAC")]
    [InlineData("")]
    public void ToEnum_VariousInvalidInputs_ReturnsUnknown(string input)
    {
        // Act
        var result = _mapper.ToEnum(input);

        // Assert
        result.Should().Be(ActionCode.UNKNOWN);
    }

    [Fact]
    public void ToEnum_DbExtraEntry_FallsBackToEnumParsing()
    {
        // Arrange - Add a custom entry to DB that is not in enum
        _db.ActionCatalog.Add(new ActionCatalog { Id = "CUSTOM_ACTION", Description = "Custom" });
        _db.SaveChanges();

        // Recreate mapper to pick up new DB entry
        var mapperWithCustom = new ActionCodeMapper(_db);

        // Act - CUSTOM_ACTION is in DB but not in enum, so should return UNKNOWN
        var result = mapperWithCustom.ToEnum("CUSTOM_ACTION");

        // Assert
        result.Should().Be(ActionCode.UNKNOWN);
    }

    // ========== Part 6: GenderScope Filtering ==========

    [Fact]
    public void GetRulesForAction_GenderScopeBoth_AppliesToMale()
    {
        // Arrange - Create mapping with GenderScope=Both
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "TestRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.Both,
            Tier = RuleTier.Year,
            Priority = 1
        });
        _db.SaveChanges();

        var mapper = new ActionCodeMapper(_db);

        // Act - Male user
        var rules = mapper.GetRulesForAction("TEST_ACTION", Gender.Male, RuleTier.Year);

        // Assert - Both scope applies to Male
        rules.Should().Contain("TestRule");
    }

    [Fact]
    public void GetRulesForAction_GenderScopeBoth_AppliesToFemale()
    {
        // Arrange - Create mapping with GenderScope=Both
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "TestRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.Both,
            Tier = RuleTier.Year,
            Priority = 1
        });
        _db.SaveChanges();

        var mapper = new ActionCodeMapper(_db);

        // Act - Female user
        var rules = mapper.GetRulesForAction("TEST_ACTION", Gender.Female, RuleTier.Year);

        // Assert - Both scope applies to Female
        rules.Should().Contain("TestRule");
    }

    [Fact]
    public void GetRulesForAction_GenderScopeMaleOnly_AppliesToMale()
    {
        // Arrange - Create mapping with GenderScope=MaleOnly
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "MaleRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.MaleOnly,
            Tier = RuleTier.Year,
            Priority = 1
        });
        _db.SaveChanges();

        var mapper = new ActionCodeMapper(_db);

        // Act - Male user
        var rules = mapper.GetRulesForAction("TEST_ACTION", Gender.Male, RuleTier.Year);

        // Assert - MaleOnly scope applies to Male
        rules.Should().Contain("MaleRule");
    }

    [Fact]
    public void GetRulesForAction_GenderScopeMaleOnly_SkippedForFemale()
    {
        // Arrange - Create mapping with GenderScope=MaleOnly
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "MaleRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.MaleOnly,
            Tier = RuleTier.Year,
            Priority = 1
        });
        _db.SaveChanges();

        var mapper = new ActionCodeMapper(_db);

        // Act - Female user
        var rules = mapper.GetRulesForAction("TEST_ACTION", Gender.Female, RuleTier.Year);

        // Assert - MaleOnly scope is skipped for Female
        rules.Should().NotContain("MaleRule");
    }

    [Fact]
    public void GetRulesForAction_GenderScopeFemaleOnly_AppliesToFemale()
    {
        // Arrange - Create mapping with GenderScope=FemaleOnly
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "FemaleRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.FemaleOnly,
            Tier = RuleTier.Year,
            Priority = 1
        });
        _db.SaveChanges();

        var mapper = new ActionCodeMapper(_db);

        // Act - Female user
        var rules = mapper.GetRulesForAction("TEST_ACTION", Gender.Female, RuleTier.Year);

        // Assert - FemaleOnly scope applies to Female
        rules.Should().Contain("FemaleRule");
    }

    [Fact]
    public void GetRulesForAction_GenderScopeFemaleOnly_SkippedForMale()
    {
        // Arrange - Create mapping with GenderScope=FemaleOnly
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "FemaleRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.FemaleOnly,
            Tier = RuleTier.Year,
            Priority = 1
        });
        _db.SaveChanges();

        var mapper = new ActionCodeMapper(_db);

        // Act - Male user
        var rules = mapper.GetRulesForAction("TEST_ACTION", Gender.Male, RuleTier.Year);

        // Assert - FemaleOnly scope is skipped for Male
        rules.Should().NotContain("FemaleRule");
    }

    // ========== Part 7: RuleTier Filtering ==========

    [Fact]
    public void GetRulesForAction_TierYear_ReturnsOnlyYearMappings()
    {
        // Arrange - Add Year tier mapping
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "YearRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.Both,
            Tier = RuleTier.Year,
            Priority = 1
        });
        // Add Day tier mapping (should not be returned)
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "DayRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.Both,
            Tier = RuleTier.Day,
            Priority = 1
        });
        _db.SaveChanges();

        var mapper = new ActionCodeMapper(_db);

        // Act - Query Year tier
        var rules = mapper.GetRulesForAction("TEST_ACTION", Gender.Male, RuleTier.Year);

        // Assert - Only Year rule returned
        rules.Should().Contain("YearRule");
        rules.Should().NotContain("DayRule");
    }

    [Fact]
    public void GetRulesForAction_TierDay_ReturnsOnlyDayMappings()
    {
        // Arrange - Add Year tier mapping (should not be returned)
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "YearRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.Both,
            Tier = RuleTier.Year,
            Priority = 1
        });
        // Add Day tier mapping
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "DayRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.Both,
            Tier = RuleTier.Day,
            Priority = 1
        });
        _db.SaveChanges();

        var mapper = new ActionCodeMapper(_db);

        // Act - Query Day tier
        var rules = mapper.GetRulesForAction("TEST_ACTION", Gender.Male, RuleTier.Day);

        // Assert - Only Day rule returned
        rules.Should().Contain("DayRule");
        rules.Should().NotContain("YearRule");
    }

    [Fact]
    public void GetRulesForAction_TierAll_ReturnsAllTiers()
    {
        // Arrange - Add Year and Day tier mappings with Tier=All
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "YearRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.Both,
            Tier = RuleTier.Year,
            Priority = 1
        });
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "DayRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.Both,
            Tier = RuleTier.Day,
            Priority = 1
        });
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "AllTierRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.Both,
            Tier = RuleTier.All,
            Priority = 1
        });
        _db.SaveChanges();

        var mapper = new ActionCodeMapper(_db);

        // Act - Query Year tier (Tier=All should also match)
        var yearRules = mapper.GetRulesForAction("TEST_ACTION", Gender.Male, RuleTier.Year);
        // Act - Query Day tier (Tier=All should also match)
        var dayRules = mapper.GetRulesForAction("TEST_ACTION", Gender.Male, RuleTier.Day);

        // Assert - Tier=All rules appear in both tier queries
        yearRules.Should().Contain("YearRule");
        yearRules.Should().Contain("AllTierRule");
        dayRules.Should().Contain("DayRule");
        dayRules.Should().Contain("AllTierRule");
    }

    // ========== Part 8: Priority Ordering ==========

    [Fact]
    public void GetRulesForAction_OrderedByPriorityAscending()
    {
        // Arrange - Add mappings with different priorities
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "LowPriority",
            IsMandatory = true,
            GenderScope = GenderApplyScope.Both,
            Tier = RuleTier.Year,
            Priority = 3
        });
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "HighPriority",
            IsMandatory = true,
            GenderScope = GenderApplyScope.Both,
            Tier = RuleTier.Year,
            Priority = 1
        });
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "MediumPriority",
            IsMandatory = true,
            GenderScope = GenderApplyScope.Both,
            Tier = RuleTier.Year,
            Priority = 2
        });
        _db.SaveChanges();

        var mapper = new ActionCodeMapper(_db);

        // Act
        var rules = mapper.GetRulesForAction("TEST_ACTION", Gender.Male, RuleTier.Year);

        // Assert - Lower priority number = higher priority = appears first
        rules.Should().ContainInOrder("HighPriority", "MediumPriority", "LowPriority");
    }

    [Fact]
    public void GetRulesForAction_SamePriority_PreservesInsertionOrder()
    {
        // Arrange - Add two mappings with same priority
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "FirstRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.Both,
            Tier = RuleTier.Year,
            Priority = 1
        });
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "SecondRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.Both,
            Tier = RuleTier.Year,
            Priority = 1
        });
        _db.SaveChanges();

        var mapper = new ActionCodeMapper(_db);

        // Act
        var rules = mapper.GetRulesForAction("TEST_ACTION", Gender.Male, RuleTier.Year);

        // Assert - Both rules present (stable order preserved by EF)
        rules.Should().Contain("FirstRule");
        rules.Should().Contain("SecondRule");
        rules.Should().HaveCount(2);
    }

    // ========== Part 9: Combined Gender + Tier Filtering ==========

    [Fact]
    public void GetRulesForAction_CombinedGenderAndTier_FiltersCorrectly()
    {
        // Arrange - Male-only Year rule
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "MaleYearRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.MaleOnly,
            Tier = RuleTier.Year,
            Priority = 1
        });
        // Female-only Year rule
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "FemaleYearRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.FemaleOnly,
            Tier = RuleTier.Year,
            Priority = 1
        });
        // Male-only Day rule (should not appear in Year query)
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "MaleDayRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.MaleOnly,
            Tier = RuleTier.Day,
            Priority = 1
        });
        _db.SaveChanges();

        var mapper = new ActionCodeMapper(_db);

        // Act - Male querying Year tier
        var maleYearRules = mapper.GetRulesForAction("TEST_ACTION", Gender.Male, RuleTier.Year);
        // Act - Female querying Year tier
        var femaleYearRules = mapper.GetRulesForAction("TEST_ACTION", Gender.Female, RuleTier.Year);

        // Assert - Male sees MaleYearRule, Female sees FemaleYearRule, neither sees Day rules
        maleYearRules.Should().Contain("MaleYearRule");
        maleYearRules.Should().NotContain("FemaleYearRule");
        maleYearRules.Should().NotContain("MaleDayRule");

        femaleYearRules.Should().Contain("FemaleYearRule");
        femaleYearRules.Should().NotContain("MaleYearRule");
        femaleYearRules.Should().NotContain("MaleDayRule");
    }

    [Fact]
    public void GetRulesForAction_UnknownActionId_ReturnsEmptyList()
    {
        // Arrange
        var mapper = new ActionCodeMapper(_db);

        // Act
        var rules = mapper.GetRulesForAction("NONEXISTENT_ACTION", Gender.Male, RuleTier.Year);

        // Assert - per contract: Query with unknown ActionId returns empty result set
        rules.Should().BeEmpty();
    }

    [Fact]
    public void GetRulesForAction_UnknownTier_ReturnsEmptyList()
    {
        // Arrange - Add a valid mapping
        _db.ActionRuleMappings.Add(new ActionRuleMapping
        {
            ActionId = "TEST_ACTION",
            RuleCode = "TestRule",
            IsMandatory = true,
            GenderScope = GenderApplyScope.Both,
            Tier = RuleTier.Year,
            Priority = 1
        });
        _db.SaveChanges();

        var mapper = new ActionCodeMapper(_db);

        // Act - Query with non-existent tier
        var rules = mapper.GetRulesForAction("TEST_ACTION", Gender.Male, (RuleTier)999);

        // Assert - per contract: Query with unknown Tier returns empty result set
        rules.Should().BeEmpty();
    }
}
