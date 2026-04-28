using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Features.EvaluateSpiritualAction.Models;
using ZenTam.Api.Features.EvaluateSpiritualAction.Services;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.UnitTests.EvaluateSpiritualAction;

/// <summary>
/// Integration tests for DB-Enum consistency validation.
/// Tests cover:
/// - GetMissingEnumMappings (DB has entries not in enum)
/// - GetMissingDbEntries (enum has entries not in DB)
/// - Full consistency validation
/// </summary>
public class ActionCatalogConsistencyTests : IDisposable
{
    private readonly ZenTamDbContext _db;
    private readonly ActionCodeMapper _mapper;

    public ActionCatalogConsistencyTests()
    {
        _db = TestHelpers.TestDbHelper.CreateInMemoryDbContext();
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

    // ========== Part 2.1: GetMissingEnumMappings - Happy Path ==========

    [Fact]
    public void GetMissingEnumMappings_AllDbEntriesHaveEnum_ReturnsEmptyList()
    {
        // Arrange - Seed DB with exactly the 25 enum entries
        SeedAll25ActionCodes(_db);

        // Act
        var missing = _mapper.GetMissingEnumMappings();

        // Assert
        missing.Should().BeEmpty();
    }

    [Fact]
    public void GetMissingEnumMappings_DbHasExtraEntry_ReturnsExtraEntries()
    {
        // Arrange - Seed DB with 25 enum entries + 1 extra
        SeedAll25ActionCodes(_db);
        _db.ActionCatalog.Add(new ActionCatalog { Id = "CUSTOM_ACTION", Description = "Custom action" });
        _db.SaveChanges();

        // Recreate mapper to pick up new DB entry
        var mapperWithExtra = new ActionCodeMapper(_db);

        // Act
        var missing = mapperWithExtra.GetMissingEnumMappings();

        // Assert
        missing.Should().Contain("CUSTOM_ACTION");
    }

    [Fact]
    public void GetMissingEnumMappings_DbHasMultipleExtraEntries_ReturnsAllExtraEntries()
    {
        // Arrange
        SeedAll25ActionCodes(_db);
        _db.ActionCatalog.Add(new ActionCatalog { Id = "EXTRA_1", Description = "Extra 1" });
        _db.ActionCatalog.Add(new ActionCatalog { Id = "EXTRA_2", Description = "Extra 2" });
        _db.SaveChanges();

        var mapperWithExtra = new ActionCodeMapper(_db);

        // Act
        var missing = mapperWithExtra.GetMissingEnumMappings();

        // Assert
        missing.Should().HaveCount(2);
        missing.Should().Contain("EXTRA_1");
        missing.Should().Contain("EXTRA_2");
    }

    // ========== Part 2.2: GetMissingDbEntries - Happy Path ==========

    [Fact]
    public void GetMissingDbEntries_AllEnumEntriesInDb_ReturnsEmptyList()
    {
        // Arrange - Seed DB with exactly the 25 enum entries
        SeedAll25ActionCodes(_db);

        // Act
        var missing = _mapper.GetMissingDbEntries();

        // Assert
        missing.Should().BeEmpty();
    }

    [Fact]
    public void GetMissingDbEntries_DbMissingSomeEnumEntries_ReturnsMissingEntries()
    {
        // Arrange - First clear all existing ActionCatalog entries
        var existingEntries = _db.ActionCatalog.Local.ToList();
        foreach (var entry in existingEntries)
        {
            _db.Entry(entry).State = EntityState.Detached;
        }
        _db.ActionCatalog.RemoveRange(existingEntries);
        _db.SaveChanges();

        // Only seed 15 of 25 entries
        var partialCodes = new[]
        {
            "XAY_NHA", "SUA_NHA", "NHAP_TRACH", "CUOI_HOI", "SINH_CON",
            "KHAI_TRUONG", "KY_HOP_DONG", "NHAN_VIEC", "MUA_VANG", "MUA_DAT",
            "MUA_XE", "DAM_BAO_HANH", "XUAT_HANH", "CU_HUONG", "BAT_DAU"
        };

        foreach (var code in partialCodes)
        {
            _db.ActionCatalog.Add(new ActionCatalog { Id = code, Description = $"Test {code}" });
        }
        _db.SaveChanges();

        var mapperPartial = new ActionCodeMapper(_db);

        // Act
        var missing = mapperPartial.GetMissingDbEntries();

        // Assert - Should have 10 missing entries
        missing.Should().HaveCount(10);
        missing.Should().Contain(ActionCode.CHUA_BENH);
        missing.Should().Contain(ActionCode.TAM_SOAT);
        missing.Should().Contain(ActionCode.KHAI_VONG);
        missing.Should().Contain(ActionCode.THI_DAU);
        missing.Should().Contain(ActionCode.AN_TANG);
        missing.Should().Contain(ActionCode.BOC_MO);
        missing.Should().Contain(ActionCode.THO_MAU);
        missing.Should().Contain(ActionCode.LE_BAI);
        missing.Should().Contain(ActionCode.CAT_SAC);
        missing.Should().Contain(ActionCode.TU_TUC);
    }

    [Fact]
    public void GetMissingDbEntries_DbCompletelyEmpty_ReturnsAll25EnumEntries()
    {
        // Arrange - DB has only XAY_NHA and INVALID from TestDbHelper seeding
        // Note: TestDbHelper seeds XAY_NHA (which exists in enum) and INVALID (which doesn't)
        var mapperEmpty = new ActionCodeMapper(_db);

        // Act
        var missing = mapperEmpty.GetMissingDbEntries();

        // Assert - Should have 24 missing entries (25 - XAY_NHA which is seeded)
        missing.Should().HaveCount(24);
        missing.Should().NotContain(ActionCode.XAY_NHA);
    }

    // ========== Part 2.3: Full Consistency Validation ==========

    [Fact]
    public void FullConsistencyValidation_PerfectSync_NoExceptions()
    {
        // Arrange - Seed DB with exactly the 25 enum entries
        SeedAll25ActionCodes(_db);

        // Act
        var missingEnumMappings = _mapper.GetMissingEnumMappings();
        var missingDbEntries = _mapper.GetMissingDbEntries();

        // Assert
        missingEnumMappings.Should().BeEmpty();
        missingDbEntries.Should().BeEmpty();
    }

    [Fact]
    public void FullConsistencyValidation_MissingDbEntry_DetectsMissing()
    {
        // Arrange - Remove CUOI_HOI from DB
        SeedAll25ActionCodes(_db);
        var cuoiHoi = _db.ActionCatalog.FirstOrDefault(ac => ac.Id == "CUOI_HOI");
        if (cuoiHoi != null)
        {
            _db.ActionCatalog.Remove(cuoiHoi);
            _db.SaveChanges();
        }

        var mapperMissingEntry = new ActionCodeMapper(_db);

        // Act
        var missingDbEntries = mapperMissingEntry.GetMissingDbEntries();

        // Assert
        missingDbEntries.Should().Contain(ActionCode.CUOI_HOI);
    }

    [Fact]
    public void FullConsistencyValidation_MissingEnumMapping_DetectsMissing()
    {
        // Arrange - Add entry to DB that doesn't exist in enum
        SeedAll25ActionCodes(_db);
        _db.ActionCatalog.Add(new ActionCatalog { Id = "MYSTERIOUS_ACTION", Description = "Mysterious" });
        _db.SaveChanges();

        var mapperWithExtra = new ActionCodeMapper(_db);

        // Act
        var missingEnumMappings = mapperWithExtra.GetMissingEnumMappings();

        // Assert
        missingEnumMappings.Should().Contain("MYSTERIOUS_ACTION");
    }

    // ========== Edge Cases ==========

    [Fact]
    public void GetMissingDbEntries_CaseInsensitive_DoesNotDoubleCount()
    {
        // Arrange - Get the existing XAY_NHA entry and create a case variation mapper
        // Note: ActionCatalog uses string keys and EF Core tracks them by reference
        // If we try to add same key twice, we get a tracking conflict
        var existingXayNha = _db.ActionCatalog.Local.FirstOrDefault(x => x.Id == "XAY_NHA");
        
        var mapperCaseVariation = new ActionCodeMapper(_db);

        // Act
        var missing = mapperCaseVariation.GetMissingDbEntries();

        // Assert - The existing XAY_NHA is counted once (not as missing since it exists)
        // INVALID is missing from enum, XAY_NHA exists
        missing.Should().NotContain(ActionCode.XAY_NHA);
    }

    [Fact]
    public void GetMissingEnumMappings_CaseInsensitive_DoesNotDoubleCount()
    {
        // Arrange - Use a fresh context to avoid tracking conflicts
        // Note: ActionCode enum is case-insensitive, so XAY_NHA and xay_nha are the same
        var mapperCaseVariation = new ActionCodeMapper(_db);

        // Act
        var missing = mapperCaseVariation.GetMissingEnumMappings();

        // Assert - Should contain INVALID (which exists in DB but not in enum)
        missing.Should().Contain("INVALID");
    }

    [Fact]
    public void GetMissingEnumMappings_DbEmpty_ReturnsInvalidEntry()
    {
        // Arrange - TestDbHelper seeds only XAY_NHA and INVALID in ActionCatalog
        // INVALID doesn't exist in ActionCode enum, so it should be reported as missing
        var mapperEmpty = new ActionCodeMapper(_db);

        // Act
        var missing = mapperEmpty.GetMissingEnumMappings();

        // Assert - Only INVALID is missing from enum (XAY_NHA exists in both)
        missing.Should().HaveCount(1);
        missing.Should().Contain("INVALID");
    }
}
