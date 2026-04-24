using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.UnitTests.TestHelpers;

public static class TestDbHelper
{
    public static readonly Guid SeedUserId = new("3c7be808-02c1-4f24-85e1-26f0f2455675");
    public static readonly Guid Male1990UserId = new("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Female1996UserId = new("22222222-2222-2222-2222-222222222222");
    public static readonly Guid Male1986UserId = new("33333333-3333-3333-3333-333333333333");

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
        // Seed test user (Male, LunarYOB=1996, SolarDOB=1996-05-15)
        context.Users.Add(new User
        {
            Id = SeedUserId,
            Username = "test_user",
            Gender = Gender.Male,
            SolarDOB = new DateTime(1996, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            LunarYOB = 1996
        });

        // Seed male user with LunarYOB=1990 for TC-02
        context.Users.Add(new User
        {
            Id = Male1990UserId,
            Username = "male_1990",
            Gender = Gender.Male,
            SolarDOB = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            LunarYOB = 1990
        });

        // Seed female user with LunarYOB=1996 for TC-03
        context.Users.Add(new User
        {
            Id = Female1996UserId,
            Username = "female_1996",
            Gender = Gender.Female,
            SolarDOB = new DateTime(1996, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            LunarYOB = 1996
        });

        // Seed male user with LunarYOB=1986 for TC-04
        context.Users.Add(new User
        {
            Id = Male1986UserId,
            Username = "male_1986",
            Gender = Gender.Male,
            SolarDOB = new DateTime(1986, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            LunarYOB = 1986
        });

        // Seed action catalog
        context.ActionCatalog.Add(new ActionCatalog { Id = "XAY_NHA", Description = "Xây nhà mới" });
        context.ActionCatalog.Add(new ActionCatalog { Id = "INVALID", Description = "Invalid action" });

        // Seed action rule mappings for XAY_NHA
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 1,
            ActionId = "XAY_NHA",
            RuleCode = "Rule_KimLau",
            IsMandatory = true,
            GenderConstraint = Gender.Male
        });
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 2,
            ActionId = "XAY_NHA",
            RuleCode = "Rule_HoangOc",
            IsMandatory = true,
            GenderConstraint = Gender.Male
        });
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 3,
            ActionId = "XAY_NHA",
            RuleCode = "Rule_TamTai",
            IsMandatory = false,
            GenderConstraint = Gender.Male
        });
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 4,
            ActionId = "XAY_NHA",
            RuleCode = "Rule_ThaiTue",
            IsMandatory = false,
            GenderConstraint = Gender.Male
        });

        context.SaveChanges();
    }
}
