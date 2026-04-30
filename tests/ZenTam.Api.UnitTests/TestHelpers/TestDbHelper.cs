using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.UnitTests.TestHelpers;

public static class TestDbHelper
{
    // Test data IDs from contract spec (Section 4)
    public static readonly Guid SeedClientId = new("3c7be808-02c1-4f24-85e1-26f0f2455675");
    public static readonly Guid Male1990Id = new("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Female1996Id = new("22222222-2222-2222-2222-222222222222");
    public static readonly Guid Male1986Id = new("33333333-3333-3333-3333-333333333333");

    public static ZenTamDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ZenTamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ZenTamDbContext(options);
        SeedTestData(context);
        return context;
    }

    public static void SeedTestData(ZenTamDbContext context)
    {
        // Seed test client for TC-01 (Male, LunarYOB=1996, SolarDOB=1996-05-15)
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = SeedClientId,
            Username = "test_user",
            Name = "Test User",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1996, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });

        // Seed male client with LunarYOB=1990 for TC-02/TC-07 (KimLau fail test)
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = Male1990Id,
            Username = "male_1990",
            Name = "Male 1990",
            PhoneNumber = "0909123457",
            SolarDob = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });

        // Seed female client with LunarYOB=1996 for TC-02 (Gender filter test)
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = Female1996Id,
            Username = "female_1996",
            Name = "Female 1996",
            PhoneNumber = "0909123458",
            SolarDob = new DateTime(1996, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Female,
            CreatedAt = DateTime.UtcNow
        });

        // Seed male client with LunarYOB=1986 for TC-04/TC-08 (TamTai fail test)
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = Male1986Id,
            Username = "male_1986",
            Name = "Male 1986",
            PhoneNumber = "0909123459",
            SolarDob = new DateTime(1986, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });

        // Seed action catalog
        context.ActionCatalog.Add(new ActionCatalog { Id = "XAY_NHA", Description = "Xây nhà mới" });
        context.ActionCatalog.Add(new ActionCatalog { Id = "INVALID", Description = "Invalid action" });

        // Seed action rule mappings for XAY_NHA
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 1,
            ActionId = "XAY_NHA",
            RuleCode = "KimLau",
            IsMandatory = true,
            GenderScope = GenderApplyScope.MaleOnly,
            Tier = RuleTier.Year,
            Priority = 1
        });
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 2,
            ActionId = "XAY_NHA",
            RuleCode = "HoangOc",
            IsMandatory = true,
            GenderScope = GenderApplyScope.MaleOnly,
            Tier = RuleTier.Year,
            Priority = 2
        });
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 3,
            ActionId = "XAY_NHA",
            RuleCode = "TamTai",
            IsMandatory = false,
            GenderScope = GenderApplyScope.Both,
            Tier = RuleTier.Year,
            Priority = 3
        });
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 4,
            ActionId = "XAY_NHA",
            RuleCode = "ThaiTue",
            IsMandatory = false,
            GenderScope = GenderApplyScope.MaleOnly,
            Tier = RuleTier.Year,
            Priority = 4
        });

        context.SaveChanges();
    }
}
