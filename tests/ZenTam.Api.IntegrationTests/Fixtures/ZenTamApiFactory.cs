using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.IntegrationTests.Fixtures;

internal static class TestSeedState
{
    internal static readonly string DatabaseName = $"ZenTamTest_{Guid.NewGuid()}";
    internal static bool Seeded = false;
    internal static readonly object SeedLock = new();
}

public class ZenTamApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ZenTamDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add in-memory database with fixed name for all test classes
            services.AddDbContext<ZenTamDbContext>(options =>
            {
                options.UseInMemoryDatabase(TestSeedState.DatabaseName);
            });
        });
    }

    /// <summary>
    /// Seeds test data on first call. Safe to call multiple times.
    /// </summary>
    public void EnsureSeeded()
    {
        if (TestSeedState.Seeded) return;

        lock (TestSeedState.SeedLock)
        {
            if (TestSeedState.Seeded) return;

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ZenTamDbContext>();
            db.Database.EnsureCreated();

            if (!db.ActionCatalog.Any())
            {
                db.ActionCatalog.AddRange(
                    new ActionCatalog { Id = "XAY_NHA", Description = "Xây nhà mới" },
                    new ActionCatalog { Id = "MAU_TOC", Description = "Cắt tóc" },
                    new ActionCatalog { Id = "KHAI_TRUONG", Description = "Khai trương" },
                    new ActionCatalog { Id = "CUOI_HOI", Description = "Cưới hỏi" },
                    new ActionCatalog { Id = "SUA_NHA", Description = "Sửa nhà" },
                    new ActionCatalog { Id = "NHAP_TRACH", Description = "Nhập trạch" },
                    new ActionCatalog { Id = "SINH_CON", Description = "Sinh con" },
                    new ActionCatalog { Id = "MUA_VANG", Description = "Mua vàng" },
                    new ActionCatalog { Id = "MUA_DAT", Description = "Mua đất" },
                    new ActionCatalog { Id = "MUA_XE", Description = "Mua xe" },
                    new ActionCatalog { Id = "DAM_BAO_HANH", Description = "Đặt bảo hành" },
                    new ActionCatalog { Id = "XUAT_HANH", Description = "Xuất hành" },
                    new ActionCatalog { Id = "CU_HUONG", Description = "Về quê" },
                    new ActionCatalog { Id = "BAT_DAU", Description = "Bắt đầu" },
                    new ActionCatalog { Id = "CHUA_BENH", Description = "Chữa bệnh" },
                    new ActionCatalog { Id = "TAM_SOAT", Description = "Tầm soát" },
                    new ActionCatalog { Id = "KHAI_VONG", Description = "Khai võng" },
                    new ActionCatalog { Id = "THI_DAU", Description = "Thi đấu" },
                    new ActionCatalog { Id = "AN_TANG", Description = "An táng" },
                    new ActionCatalog { Id = "BOC_MO", Description = "Bốc mộ" },
                    new ActionCatalog { Id = "THO_MAU", Description = "Thổ mộ" },
                    new ActionCatalog { Id = "LE_BAI", Description = "Lễ bái" },
                    new ActionCatalog { Id = "CAT_SAC", Description = "Cắt sắc" },
                    new ActionCatalog { Id = "TU_TUC", Description = "Tự tức" },
                    new ActionCatalog { Id = "KY_HOP_DONG", Description = "Ký hợp đồng" },
                    new ActionCatalog { Id = "NHAN_VIEC", Description = "Nhận việc" }
                );
            }

            if (!db.ActionRuleMappings.Any())
            {
                db.ActionRuleMappings.AddRange(
                    // XAY_NHA - Year
                    new ActionRuleMapping { ActionId = "XAY_NHA", RuleCode = "KIM_LAU", IsMandatory = true, GenderScope = GenderApplyScope.Both, Tier = RuleTier.Year, Priority = 1 },
                    new ActionRuleMapping { ActionId = "XAY_NHA", RuleCode = "HOANG_OC", IsMandatory = true, GenderScope = GenderApplyScope.Both, Tier = RuleTier.Year, Priority = 2 },
                    new ActionRuleMapping { ActionId = "XAY_NHA", RuleCode = "TAM_TAI", IsMandatory = false, GenderScope = GenderApplyScope.Both, Tier = RuleTier.Year, Priority = 3 },
                    new ActionRuleMapping { ActionId = "XAY_NHA", RuleCode = "THAI_TUE", IsMandatory = false, GenderScope = GenderApplyScope.Both, Tier = RuleTier.Year, Priority = 4 },
                    // CUOI_HOI
                    new ActionRuleMapping { ActionId = "CUOI_HOI", RuleCode = "KIM_LAU", IsMandatory = true, GenderScope = GenderApplyScope.FemaleOnly, Tier = RuleTier.Year, Priority = 1 },
                    new ActionRuleMapping { ActionId = "CUOI_HOI", RuleCode = "TAM_TAI", IsMandatory = false, GenderScope = GenderApplyScope.FemaleOnly, Tier = RuleTier.Year, Priority = 2 },
                    new ActionRuleMapping { ActionId = "CUOI_HOI", RuleCode = "HOANG_OC", IsMandatory = false, GenderScope = GenderApplyScope.Both, Tier = RuleTier.Year, Priority = 3 },
                    // KHAI_TRUONG
                    new ActionRuleMapping { ActionId = "KHAI_TRUONG", RuleCode = "TAM_TAI", IsMandatory = true, GenderScope = GenderApplyScope.Both, Tier = RuleTier.Year, Priority = 1 },
                    new ActionRuleMapping { ActionId = "KHAI_TRUONG", RuleCode = "THAI_TUE", IsMandatory = true, GenderScope = GenderApplyScope.Both, Tier = RuleTier.Year, Priority = 2 },
                    // SUA_NHA
                    new ActionRuleMapping { ActionId = "SUA_NHA", RuleCode = "KIM_LAU", IsMandatory = true, GenderScope = GenderApplyScope.Both, Tier = RuleTier.Year, Priority = 1 },
                    // NHAP_TRACH
                    new ActionRuleMapping { ActionId = "NHAP_TRACH", RuleCode = "TAM_TAI", IsMandatory = true, GenderScope = GenderApplyScope.Both, Tier = RuleTier.Year, Priority = 1 },
                    // SINH_CON
                    new ActionRuleMapping { ActionId = "SINH_CON", RuleCode = "KIM_LAU", IsMandatory = true, GenderScope = GenderApplyScope.FemaleOnly, Tier = RuleTier.Year, Priority = 1 },
                    // MUA_XE
                    new ActionRuleMapping { ActionId = "MUA_XE", RuleCode = "TAM_TAI", IsMandatory = false, GenderScope = GenderApplyScope.Both, Tier = RuleTier.Year, Priority = 1 },
                    // MUA_DAT
                    new ActionRuleMapping { ActionId = "MUA_DAT", RuleCode = "KIM_LAU", IsMandatory = true, GenderScope = GenderApplyScope.Both, Tier = RuleTier.Year, Priority = 1 },
                    // NHAN_VIEC
                    new ActionRuleMapping { ActionId = "NHAN_VIEC", RuleCode = "THAI_TUE", IsMandatory = true, GenderScope = GenderApplyScope.Both, Tier = RuleTier.Year, Priority = 1 },
                    // MAU_TOC - Month
                    new ActionRuleMapping { ActionId = "MAU_TOC", RuleCode = "NGUYET_KY", IsMandatory = true, GenderScope = GenderApplyScope.Both, Tier = RuleTier.Month, Priority = 1 }
                );
            }

            db.SaveChanges();
            TestSeedState.Seeded = true;
        }
    }
}