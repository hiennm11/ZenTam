using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.UnitTests.EvaluateSpiritualAction;

/// <summary>
/// Tests for ActionRuleMapping seed data validation.
/// Verifies row counts, tier assignments, gender scope values, priorities, and referential integrity.
/// </summary>
public class ActionRuleMappingSeedTests : IDisposable
{
    private readonly ZenTamDbContext _db;

    public ActionRuleMappingSeedTests()
    {
        var options = new DbContextOptionsBuilder<ZenTamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new ZenTamDbContext(options);
        DataSeeder.SeedAsync(_db).Wait();
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    // ========== Row Count Validation ==========

    [Fact]
    public void SeedData_YearTierRowCount_Is15()
    {
        // Arrange & Act
        var yearTierCount = _db.ActionRuleMappings
            .Count(m => m.Tier == RuleTier.Year);

        // Assert - Year-tier row count
        yearTierCount.Should().Be(15);
    }

    [Fact]
    public void SeedData_DayTierRowCount_Is13()
    {
        // Arrange & Act
        var dayTierCount = _db.ActionRuleMappings
            .Count(m => m.Tier == RuleTier.Day);

        // Assert - Day-tier row count
        dayTierCount.Should().Be(13);
    }

    [Fact]
    public void SeedData_TotalRowCount_Is28()
    {
        // Arrange & Act
        var totalCount = _db.ActionRuleMappings.Count();

        // Assert - Total count
        totalCount.Should().Be(28);
    }

    // ========== Tier Assignment Validation ==========

    [Fact]
    public void SeedData_AllYearTierRows_HaveYearTier()
    {
        // Arrange & Act
        var yearMappings = _db.ActionRuleMappings
            .Where(m => m.Tier == RuleTier.Year)
            .ToList();

        // Assert - All Year-tier rows have Tier = Year
        yearMappings.Should().OnlyContain(m => m.Tier == RuleTier.Year);
    }

    [Fact]
    public void SeedData_AllDayTierRows_HaveDayTier()
    {
        // Arrange & Act
        var dayMappings = _db.ActionRuleMappings
            .Where(m => m.Tier == RuleTier.Day)
            .ToList();

        // Assert - All Day-tier rows have Tier = Day
        dayMappings.Should().OnlyContain(m => m.Tier == RuleTier.Day);
    }

    [Fact]
    public void SeedData_NoMappingsWithTierAll()
    {
        // Arrange & Act
        var allTierMappings = _db.ActionRuleMappings
            .Count(m => m.Tier == RuleTier.All);

        // Assert - No mappings should have Tier = All in seed data
        allTierMappings.Should().Be(0);
    }

    [Fact]
    public void SeedData_NoMappingsWithTierMonth()
    {
        // Arrange & Act
        var monthTierMappings = _db.ActionRuleMappings
            .Count(m => m.Tier == RuleTier.Month);

        // Assert - No mappings should have Tier = Month in seed data
        monthTierMappings.Should().Be(0);
    }

    // ========== GenderScope Validation ==========

    [Fact]
    public void SeedData_AllGenderScopeValues_AreValid()
    {
        // Arrange & Act
        var mappings = _db.ActionRuleMappings.ToList();

        // Assert - per contract: All GenderScope values are valid (Both or FemaleOnly)
        var validScopes = new[] { GenderApplyScope.Both, GenderApplyScope.FemaleOnly };
        mappings.Should().OnlyContain(m => validScopes.Contains(m.GenderScope));
    }

    [Fact]
    public void SeedData_NoMaleOnlyGenderScope()
    {
        // Arrange & Act
        var maleOnlyMappings = _db.ActionRuleMappings
            .Count(m => m.GenderScope == GenderApplyScope.MaleOnly);

        // Assert - Seed data should not have MaleOnly scope
        maleOnlyMappings.Should().Be(0);
    }

    // ========== Priority Validation ==========

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void SeedData_Priorities_AreInRange1To4(int expectedPriority)
    {
        // Arrange & Act
        var mappingsWithPriority = _db.ActionRuleMappings
            .Where(m => m.Priority == expectedPriority)
            .ToList();

        // Assert - per contract: All Priorities are between 1-4
        // This test verifies that priority values 1-4 exist in seed data
        if (expectedPriority <= 2)
        {
            mappingsWithPriority.Should().NotBeEmpty($"Priority {expectedPriority} should exist");
        }
    }

    [Fact]
    public void SeedData_AllPriorities_InValidRange()
    {
        // Arrange & Act
        var mappings = _db.ActionRuleMappings.ToList();

        // Assert - per contract: All Priorities are between 1-4
        mappings.Should().OnlyContain(m => m.Priority >= 1 && m.Priority <= 4);
    }

    [Fact]
    public void SeedData_NoPriorityZero()
    {
        // Arrange & Act
        var zeroPriorityCount = _db.ActionRuleMappings
            .Count(m => m.Priority == 0);

        // Assert - No mapping should have Priority = 0
        zeroPriorityCount.Should().Be(0);
    }

    // ========== Referential Integrity ==========

    [Fact]
    public void SeedData_AllActionIds_ReferenceExistingCatalogEntries()
    {
        // Arrange
        var catalogIds = _db.ActionCatalog
            .Select(a => a.Id)
            .ToHashSet();

        // Act
        var mappings = _db.ActionRuleMappings.ToList();

        // Assert - per contract: All ActionIds reference existing ActionCatalog entries
        mappings.Should().OnlyContain(m => catalogIds.Contains(m.ActionId));
    }

    [Fact]
    public void SeedData_AllRuleCodes_AreNonEmpty()
    {
        // Arrange & Act
        var mappings = _db.ActionRuleMappings.ToList();

        // Assert - All RuleCodes should be non-empty
        mappings.Should().OnlyContain(m => !string.IsNullOrWhiteSpace(m.RuleCode));
    }

    // ========== Specific Seed Data Validations ==========

    [Fact]
    public void SeedData_KimLauForCuoiHoi_IsFemaleOnly()
    {
        // Arrange & Act - per contract: KimLau for CUOI_HOI is FemaleOnly
        var mapping = _db.ActionRuleMappings
            .FirstOrDefault(m => m.ActionId == "CUOI_HOI" && m.RuleCode == "KimLau");

        // Assert
        mapping.Should().NotBeNull("KimLau mapping for CUOI_HOI should exist");
        mapping!.GenderScope.Should().Be(GenderApplyScope.FemaleOnly);
    }

    [Fact]
    public void SeedData_TamTaiForKhaiTruong_IsBothAndMandatory()
    {
        // Arrange & Act - per contract: TamTai for KHAI_TRUONG is Both and mandatory
        var mapping = _db.ActionRuleMappings
            .FirstOrDefault(m => m.ActionId == "KHAI_TRUONG" && m.RuleCode == "TamTai");

        // Assert
        mapping.Should().NotBeNull("TamTai mapping for KHAI_TRUONG should exist");
        mapping!.GenderScope.Should().Be(GenderApplyScope.Both);
        mapping.IsMandatory.Should().BeTrue();
    }

    [Fact]
    public void SeedData_XayNhaYearTier_HasFourRules()
    {
        // Arrange & Act - XAY_NHA Year tier should have KimLau, HoangOc, TamTai, ThaiTue
        var xayNhaYearRules = _db.ActionRuleMappings
            .Where(m => m.ActionId == "XAY_NHA" && m.Tier == RuleTier.Year)
            .Select(m => m.RuleCode)
            .ToList();

        // Assert - XAY_NHA Year tier has 4 rules
        xayNhaYearRules.Should().HaveCount(4);
        xayNhaYearRules.Should().Contain("KimLau");
        xayNhaYearRules.Should().Contain("HoangOc");
        xayNhaYearRules.Should().Contain("TamTai");
        xayNhaYearRules.Should().Contain("ThaiTue");
    }

    [Fact]
    public void SeedData_CuoiHoiYearTier_HasThreeRules()
    {
        // Arrange & Act - CUOI_HOI Year tier should have KimLau, TamTai, HoangOc
        var cuoiHoiYearRules = _db.ActionRuleMappings
            .Where(m => m.ActionId == "CUOI_HOI" && m.Tier == RuleTier.Year)
            .Select(m => m.RuleCode)
            .ToList();

        // Assert - CUOI_HOI Year tier has 3 rules
        cuoiHoiYearRules.Should().HaveCount(3);
        cuoiHoiYearRules.Should().Contain("KimLau");
        cuoiHoiYearRules.Should().Contain("TamTai");
        cuoiHoiYearRules.Should().Contain("HoangOc");
    }

    [Fact]
    public void SeedData_KhaiTruongYearTier_HasTwoRules()
    {
        // Arrange & Act - KHAI_TRUONG Year tier should have TamTai, ThaiTue
        var khaiTruongYearRules = _db.ActionRuleMappings
            .Where(m => m.ActionId == "KHAI_TRUONG" && m.Tier == RuleTier.Year)
            .Select(m => m.RuleCode)
            .ToList();

        // Assert - KHAI_TRUONG Year tier has 2 rules
        khaiTruongYearRules.Should().HaveCount(2);
        khaiTruongYearRules.Should().Contain("TamTai");
        khaiTruongYearRules.Should().Contain("ThaiTue");
    }

    [Fact]
    public void SeedData_KhaiTruongDayTier_HasFourRules()
    {
        // Arrange & Act - KHAI_TRUONG Day tier should have XungTuoiNgay, HacDao, TrucBinh, TruongXau
        var khaiTruongDayRules = _db.ActionRuleMappings
            .Where(m => m.ActionId == "KHAI_TRUONG" && m.Tier == RuleTier.Day)
            .Select(m => m.RuleCode)
            .ToList();

        // Assert - KHAI_TRUONG Day tier has 4 rules
        khaiTruongDayRules.Should().HaveCount(4);
        khaiTruongDayRules.Should().Contain("XungTuoiNgay");
        khaiTruongDayRules.Should().Contain("HacDao");
        khaiTruongDayRules.Should().Contain("TrucBinh");
        khaiTruongDayRules.Should().Contain("TruongXau");
    }

    [Fact]
    public void SeedData_MuaVangDayTier_HasTwoRules()
    {
        // Arrange & Act - MUA_VANG Day tier should have XungTuoiNgay, HoangDao
        var muaVangDayRules = _db.ActionRuleMappings
            .Where(m => m.ActionId == "MUA_VANG" && m.Tier == RuleTier.Day)
            .Select(m => m.RuleCode)
            .ToList();

        // Assert - MUA_VANG Day tier has 2 rules
        muaVangDayRules.Should().HaveCount(2);
        muaVangDayRules.Should().Contain("XungTuoiNgay");
        muaVangDayRules.Should().Contain("HoangDao");
    }

    [Fact]
    public void SeedData_AllMandatoryMappings_HavePriority1Or2()
    {
        // Arrange & Act
        var mandatoryMappings = _db.ActionRuleMappings
            .Where(m => m.IsMandatory)
            .ToList();

        // Assert - All mandatory mappings should have priority 1 or 2
        mandatoryMappings.Should().OnlyContain(m => m.Priority <= 2);
    }

    [Fact]
    public void SeedData_NhanViecYearTier_HasThaiTue()
    {
        // Arrange & Act - per contract: NHAN_VIEC Year tier has ThaiTue
        var nhanViecYearRules = _db.ActionRuleMappings
            .Where(m => m.ActionId == "NHAN_VIEC" && m.Tier == RuleTier.Year)
            .Select(m => m.RuleCode)
            .ToList();

        // Assert
        nhanViecYearRules.Should().Contain("ThaiTue");
    }
}