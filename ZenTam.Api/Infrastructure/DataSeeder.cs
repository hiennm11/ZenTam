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
        // 1. TẠO TEST USER (Giữ nguyên luồng của mày)
        if (!db.Users.Any())
        {
            var testUserId = new Guid("3c7be808-02c1-4f24-85e1-26f0f2455675");
            db.Users.Add(new User
            {
                Id = testUserId,
                Username = "hien",
                Gender = Gender.Male,
                SolarDOB = new DateTime(1998, 4, 13, 0, 0, 0, DateTimeKind.Utc),
                LunarYOB = 1998
            });
            Console.WriteLine($"Seeded test user: hien (Male - 1998)");
        }

        if (!db.ActionCatalog.Any())
        {
            db.ActionCatalog.AddRange(
                // --- NHÓM GIA ĐẠO & ĐẠI SỰ ---
                new ActionCatalog { Id = "XAY_NHA", Description = "Xây nhà mới, cất nóc, động thổ" },
                new ActionCatalog { Id = "SUA_NHA", Description = "Sửa chữa, tu tạo nhà cửa" },
                new ActionCatalog { Id = "NHAP_TRACH", Description = "Vào nhà mới, chuyển chỗ ở" },
                new ActionCatalog { Id = "CUOI_HOI", Description = "Cưới vợ, gả chồng, kết hôn" },
                new ActionCatalog { Id = "SINH_CON", Description = "Dự định sinh con đẻ cái" },

                // --- NHÓM SỰ NGHIỆP & TÀI LỘC ---
                new ActionCatalog { Id = "KHAI_TRUONG", Description = "Khai trương cửa hàng, công ty" },
                new ActionCatalog { Id = "KY_HOP_DONG", Description = "Ký kết giao dịch, hợp đồng lớn" },
                new ActionCatalog { Id = "NHAN_VIEC", Description = "Nhận việc mới, nhậm chức, thăng chức" },

                // --- NHÓM TÀI SẢN & DI CHUYỂN ---
                new ActionCatalog { Id = "MUA_XE", Description = "Mua xe máy, ô tô" },
                new ActionCatalog { Id = "MUA_DAT", Description = "Mua bán đất đai, bất động sản" },
                new ActionCatalog { Id = "XUAT_HANH", Description = "Đi công tác xa, du học, xuất ngoại" },

                // --- NHÓM ÂM PHẦN (Chuẩn bị sẵn cho Phase 2 - Level Ngày) ---
                new ActionCatalog { Id = "AN_TANG", Description = "Chôn cất, viếng tang" },
                new ActionCatalog { Id = "BOC_MO", Description = "Sang cát, tu tạo lăng mộ" }
            );
        }

        if (!db.ActionRuleMappings.Any())
        {
            db.ActionRuleMappings.AddRange(
                // 1. RULE CHO XÂY DỰNG (Áp tuổi Nam)
                new ActionRuleMapping { ActionId = "XAY_NHA", RuleCode = "Rule_KimLau", IsMandatory = true, GenderConstraint = Gender.Male },
                new ActionRuleMapping { ActionId = "XAY_NHA", RuleCode = "Rule_HoangOc", IsMandatory = true, GenderConstraint = Gender.Male },
                new ActionRuleMapping { ActionId = "XAY_NHA", RuleCode = "Rule_TamTai", IsMandatory = false, GenderConstraint = Gender.Male }, // Cảnh báo
                new ActionRuleMapping { ActionId = "XAY_NHA", RuleCode = "Rule_ThaiTue", IsMandatory = false, GenderConstraint = Gender.Male },

                new ActionRuleMapping { ActionId = "SUA_NHA", RuleCode = "Rule_HoangOc", IsMandatory = true, GenderConstraint = Gender.Male },
                new ActionRuleMapping { ActionId = "SUA_NHA", RuleCode = "Rule_TamTai", IsMandatory = false, GenderConstraint = Gender.Male },

                new ActionRuleMapping { ActionId = "NHAP_TRACH", RuleCode = "Rule_HoangOc", IsMandatory = true, GenderConstraint = Gender.Male },

                // 2. RULE CHO CƯỚI HỎI (Áp tuổi Nữ)
                new ActionRuleMapping { ActionId = "CUOI_HOI", RuleCode = "Rule_KimLau", IsMandatory = true, GenderConstraint = Gender.Female },
                new ActionRuleMapping { ActionId = "CUOI_HOI", RuleCode = "Rule_TamTai", IsMandatory = false, GenderConstraint = Gender.Female },

                // 3. RULE CHO SINH CON (Xem tuổi bố mẹ có kỵ Thái Tuế/Tam Tai năm đó không)
                new ActionRuleMapping { ActionId = "SINH_CON", RuleCode = "Rule_ThaiTue", IsMandatory = false, GenderConstraint = null },
                new ActionRuleMapping { ActionId = "SINH_CON", RuleCode = "Rule_TamTai", IsMandatory = false, GenderConstraint = null },

                // 4. RULE CHO SỰ NGHIỆP & TÀI SẢN (Không phân biệt Nam/Nữ, kỵ Tam Tai & Thái Tuế)
                new ActionRuleMapping { ActionId = "KHAI_TRUONG", RuleCode = "Rule_TamTai", IsMandatory = true, GenderConstraint = null },
                new ActionRuleMapping { ActionId = "KHAI_TRUONG", RuleCode = "Rule_ThaiTue", IsMandatory = true, GenderConstraint = null },

                new ActionRuleMapping { ActionId = "MUA_XE", RuleCode = "Rule_TamTai", IsMandatory = true, GenderConstraint = null },
                new ActionRuleMapping { ActionId = "MUA_DAT", RuleCode = "Rule_TamTai", IsMandatory = true, GenderConstraint = null },
                new ActionRuleMapping { ActionId = "NHAN_VIEC", RuleCode = "Rule_ThaiTue", IsMandatory = false, GenderConstraint = null }

            // 5. PHASE 2 - CÁC RULE LEVEL NGÀY (Sẽ nhét vào sau)
            // new ActionRuleMapping { ActionId = "XUAT_HANH", RuleCode = "Rule_NgayXungTuoi", IsMandatory = true... }
            // new ActionRuleMapping { ActionId = "AN_TANG", RuleCode = "Rule_NgayTrungTang", IsMandatory = true... }
            );
        }

        await db.SaveChangesAsync();
    }
}
