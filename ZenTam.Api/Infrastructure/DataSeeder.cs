using ZenTam.Api.Common.Domain;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.Infrastructure;

public static class DataSeeder
{
    /// <summary>The Guid assigned to the seeded test user. Available after <see cref="SeedAsync"/> completes.</summary>
    public static Guid TestUserId { get; private set; }

    /// <summary>
    /// Idempotently seeds the in-memory database.
    /// Each collection is only populated when it contains no rows.
    /// </summary>
    public static async Task SeedAsync(ZenTamDbContext db)
    {
        if (!db.Users.Any())
        {
            TestUserId = new Guid("3c7be808-02c1-4f24-85e1-26f0f2455675");
            db.Users.Add(new User
            {
                Id       = TestUserId,
                Username = "hien",
                Gender   = Gender.Male,
                SolarDOB = new DateTime(1998, 4, 13, 0, 0, 0, DateTimeKind.Utc),
                LunarYOB = 1998
            });

            Console.WriteLine($"Seeded test user with Id: {TestUserId}");
        }

        if (!db.ActionCatalog.Any())
        {
            db.ActionCatalog.AddRange(
                new ActionCatalog { Id = "XAY_NHA",  Description = "Xây nhà"  },
                new ActionCatalog { Id = "CUOI_HOI", Description = "Cưới hỏi" }
            );
        }

        if (!db.ActionRuleMappings.Any())
        {
            db.ActionRuleMappings.AddRange(
                new ActionRuleMapping { ActionId = "XAY_NHA",  RuleCode = "Rule_KimLau",  IsMandatory = true,  GenderConstraint = Gender.Male   },
                new ActionRuleMapping { ActionId = "XAY_NHA",  RuleCode = "Rule_HoangOc", IsMandatory = true,  GenderConstraint = Gender.Male   },
                new ActionRuleMapping { ActionId = "XAY_NHA",  RuleCode = "Rule_TamTai",  IsMandatory = false, GenderConstraint = null          },
                new ActionRuleMapping { ActionId = "CUOI_HOI", RuleCode = "Rule_KimLau",  IsMandatory = true,  GenderConstraint = Gender.Female },
                new ActionRuleMapping { ActionId = "CUOI_HOI", RuleCode = "Rule_TamTai",  IsMandatory = false, GenderConstraint = null          }
            );
        }

        await db.SaveChangesAsync();
    }
}
