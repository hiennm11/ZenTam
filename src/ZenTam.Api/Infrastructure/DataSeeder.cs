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
        // 1. TẠO TEST CLIENT PROFILE "HIEN" VỚI 6 RELATED PERSONS
        if (!db.ClientProfiles.Any())
        {
            var hienId = new Guid("3c7be808-02c1-4f24-85e1-26f0f2455675");
            var hien = new ClientProfile
            {
                Id = hienId,
                Username = "hien",
                Name = "hien",
                PhoneNumber = "",
                SolarDob = new DateTime(1998, 4, 13, 0, 0, 0, DateTimeKind.Utc),
                Gender = Gender.Male,
                CreatedAt = DateTime.UtcNow
            };

            // Add 6 related persons
            hien.RelatedPersons = new List<ClientRelatedPerson>
            {
                // dung - vợ (wife)
                new ClientRelatedPerson
                {
                    Id = Guid.NewGuid(),
                    ClientId = hienId,
                    Label = "VỢ",
                    SolarDob = new DateTime(1998, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                    Gender = Gender.Female,
                    CreatedAt = DateTime.UtcNow
                },
                // quy - bố (father)
                new ClientRelatedPerson
                {
                    Id = Guid.NewGuid(),
                    ClientId = hienId,
                    Label = "BỐ",
                    SolarDob = new DateTime(1966, 9, 3, 0, 0, 0, DateTimeKind.Utc),
                    Gender = Gender.Male,
                    CreatedAt = DateTime.UtcNow
                },
                // huong - mẹ (mother)
                new ClientRelatedPerson
                {
                    Id = Guid.NewGuid(),
                    ClientId = hienId,
                    Label = "MẸ",
                    SolarDob = new DateTime(1975, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                    Gender = Gender.Female,
                    CreatedAt = DateTime.UtcNow
                },
                // hung - anh (older brother)
                new ClientRelatedPerson
                {
                    Id = Guid.NewGuid(),
                    ClientId = hienId,
                    Label = "ANH",
                    SolarDob = new DateTime(1993, 8, 26, 0, 0, 0, DateTimeKind.Utc),
                    Gender = Gender.Male,
                    CreatedAt = DateTime.UtcNow
                },
                // huyen - em gái (younger sister)
                new ClientRelatedPerson
                {
                    Id = Guid.NewGuid(),
                    ClientId = hienId,
                    Label = "EM",
                    SolarDob = new DateTime(2002, 10, 13, 0, 0, 0, DateTimeKind.Utc),
                    Gender = Gender.Female,
                    CreatedAt = DateTime.UtcNow
                },
                // huy - em trai (younger brother)
                new ClientRelatedPerson
                {
                    Id = Guid.NewGuid(),
                    ClientId = hienId,
                    Label = "EM",
                    SolarDob = new DateTime(2014, 11, 3, 0, 0, 0, DateTimeKind.Utc),
                    Gender = Gender.Male,
                    CreatedAt = DateTime.UtcNow
                }
            };

            db.ClientProfiles.Add(hien);
            Console.WriteLine($"Seeded test client: hien (Male - 1998) with 6 related persons");
        }

        if (!db.ActionCatalog.Any())
        {
            db.ActionCatalog.AddRange(
                // --- GIA DINH ---
                new ActionCatalog { Id = "XAY_NHA",      Description = "Xây nhà mới, cất nóc, động thổ" },
                new ActionCatalog { Id = "SUA_NHA",      Description = "Sửa chữa, tu tạo nhà cửa" },
                new ActionCatalog { Id = "NHAP_TRACH",   Description = "Vào nhà mới, chuyển chỗ ở" },
                new ActionCatalog { Id = "CUOI_HOI",     Description = "Cưới vợ, gả chồng, kết hôn" },
                new ActionCatalog { Id = "SINH_CON",     Description = "Dự định sinh con đẻ cái" },

                // --- NGHIEP_TAI ---
                new ActionCatalog { Id = "KHAI_TRUONG",  Description = "Khai trương cửa hàng, công ty" },
                new ActionCatalog { Id = "KY_HOP_DONG",  Description = "Ký kết giao dịch, hợp đồng lớn" },
                new ActionCatalog { Id = "NHAN_VIEC",    Description = "Nhận việc mới, nhậm chức, thăng chức" },
                new ActionCatalog { Id = "MUA_VANG",     Description = "Mua vàng, trang sức kim hoàn" },
                new ActionCatalog { Id = "MUA_DAT",      Description = "Mua bán đất đai, bất động sản" },
                new ActionCatalog { Id = "MUA_XE",       Description = "Mua xe máy, ô tô" },
                new ActionCatalog { Id = "DAM_BAO_HANH", Description = "Đặt bảo hành, ký bảo lãnh" },

                // --- DI_CHUYEN ---
                new ActionCatalog { Id = "XUAT_HANH",    Description = "Đi công tác xa, du học, xuất ngoại" },
                new ActionCatalog { Id = "CU_HUONG",     Description = "Về quê, cứ hương, thăm viếng tổ tiên" },
                new ActionCatalog { Id = "BAT_DAU",      Description = "Bắt đầu hành trình, khởi sự" },

                // --- SUC_KHOE ---
                new ActionCatalog { Id = "CHUA_BENH",    Description = "Chữa bệnh, khám chữa tại bệnh viện" },
                new ActionCatalog { Id = "TAM_SOAT",     Description = "Tầm soát, kiểm tra sức khỏe định kỳ" },

                // --- HOC_TAP ---
                new ActionCatalog { Id = "KHAI_VONG",    Description = "Khai võng, khai giảng năm học mới" },
                new ActionCatalog { Id = "THI_DAU",      Description = "Thi cử, tham gia cuộc thi" },

                // --- AM_PHAN ---
                new ActionCatalog { Id = "AN_TANG",      Description = "An táng, chôn cất" },
                new ActionCatalog { Id = "BOC_MO",       Description = "Bốc mộ, sang cát, tu tạo lăng mộ" },
                new ActionCatalog { Id = "THO_MAU",      Description = "Thổ mộ, tìm kiếm đất đặt mộ" },

                // --- TAM_LINH ---
                new ActionCatalog { Id = "LE_BAI",       Description = "Lễ bái, tảo mộ, cầu an" },
                new ActionCatalog { Id = "CAT_SAC",      Description = "Cắt sắc, hóa giải, tẩy uế" },

                // --- KHAC ---
                new ActionCatalog { Id = "TU_TUC",       Description = "Tự tứ, thiền định, tu tâm" }
            );
        }

        if (!db.ActionRuleMappings.Any())
        {
            db.ActionRuleMappings.AddRange(
                // ============================================================
                // YEAR TIER RULES (Tier = RuleTier.Year)
                // ============================================================
                // XAY_NHA Year rules (Both gender, Priority 1-3)
                new ActionRuleMapping { ActionId = "XAY_NHA",     RuleCode = "KimLau",   IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Year, Priority = 1 },
                new ActionRuleMapping { ActionId = "XAY_NHA",     RuleCode = "HoangOc",  IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Year, Priority = 2 },
                new ActionRuleMapping { ActionId = "XAY_NHA",     RuleCode = "TamTai",   IsMandatory = false, GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Year, Priority = 3 },
                new ActionRuleMapping { ActionId = "XAY_NHA",     RuleCode = "ThaiTue",  IsMandatory = false, GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Year, Priority = 4 },
                // CUOI_HOI Year rules (FemaleOnly + Both)
                new ActionRuleMapping { ActionId = "CUOI_HOI",    RuleCode = "KimLau",   IsMandatory = true,  GenderScope = GenderApplyScope.FemaleOnly, Tier = RuleTier.Year, Priority = 1 },
                new ActionRuleMapping { ActionId = "CUOI_HOI",    RuleCode = "TamTai",   IsMandatory = false, GenderScope = GenderApplyScope.FemaleOnly, Tier = RuleTier.Year, Priority = 2 },
                new ActionRuleMapping { ActionId = "CUOI_HOI",    RuleCode = "HoangOc",  IsMandatory = false, GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Year, Priority = 3 },
                // KHAI_TRUONG Year rules
                new ActionRuleMapping { ActionId = "KHAI_TRUONG",RuleCode = "TamTai",   IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Year, Priority = 1 },
                new ActionRuleMapping { ActionId = "KHAI_TRUONG",RuleCode = "ThaiTue",  IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Year, Priority = 2 },
                // SUA_NHA Year rule
                new ActionRuleMapping { ActionId = "SUA_NHA",     RuleCode = "KimLau",   IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Year, Priority = 1 },
                // NHAP_TRACH Year rule
                new ActionRuleMapping { ActionId = "NHAP_TRACH",  RuleCode = "TamTai",   IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Year, Priority = 1 },
                // SINH_CON Year rule
                new ActionRuleMapping { ActionId = "SINH_CON",    RuleCode = "KimLau",   IsMandatory = true,  GenderScope = GenderApplyScope.FemaleOnly, Tier = RuleTier.Year, Priority = 1 },
                // MUA_XE Year rule
                new ActionRuleMapping { ActionId = "MUA_XE",      RuleCode = "TamTai",   IsMandatory = false, GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Year, Priority = 1 },
                // MUA_DAT Year rule
                new ActionRuleMapping { ActionId = "MUA_DAT",     RuleCode = "KimLau",   IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Year, Priority = 1 },
                // NHAN_VIEC Year rule
                new ActionRuleMapping { ActionId = "NHAN_VIEC",   RuleCode = "ThaiTue",  IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Year, Priority = 1 },

                // ============================================================
                // MONTH TIER RULES (Tier = RuleTier.Month)
                // ============================================================
                // XAY_NHA Month rules - NguyetKy, TamNuong, XungTuoi, TamSatThang
                // Mandatory rules (XungTuoi, TamSatThang) have priority 1-2, optional rules have 3-4
                new ActionRuleMapping { ActionId = "XAY_NHA",     RuleCode = "XUNG_TUOI",     IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Month, Priority = 1 },
                new ActionRuleMapping { ActionId = "XAY_NHA",     RuleCode = "TAM_SAT_THANG", IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Month, Priority = 2 },
                new ActionRuleMapping { ActionId = "XAY_NHA",     RuleCode = "NGUYET_KY",     IsMandatory = false, GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Month, Priority = 3 },
                new ActionRuleMapping { ActionId = "XAY_NHA",     RuleCode = "TAM_NUONG",     IsMandatory = false, GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Month, Priority = 4 },

                // CUOI_HOI Month rules - Wedding is highly sensitive to monthly taboos
                // All mandatory rules have priority 1-2, optional rule has priority 3
                new ActionRuleMapping { ActionId = "CUOI_HOI",    RuleCode = "NGUYET_KY",     IsMandatory = true,  GenderScope = GenderApplyScope.FemaleOnly, Tier = RuleTier.Month, Priority = 1 },
                new ActionRuleMapping { ActionId = "CUOI_HOI",    RuleCode = "XUNG_TUOI",     IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Month, Priority = 1 },
                new ActionRuleMapping { ActionId = "CUOI_HOI",    RuleCode = "TAM_SAT_THANG", IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Month, Priority = 2 },
                new ActionRuleMapping { ActionId = "CUOI_HOI",    RuleCode = "TAM_NUONG",     IsMandatory = false, GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Month, Priority = 3 },

                // KHAI_TRUONG Month rules - Business opening
                // Mandatory rules have priority 1-2, optional rules have priority 3-4
                new ActionRuleMapping { ActionId = "KHAI_TRUONG", RuleCode = "NGUYET_KY",     IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Month, Priority = 1 },
                new ActionRuleMapping { ActionId = "KHAI_TRUONG", RuleCode = "XUNG_TUOI",     IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Month, Priority = 2 },
                new ActionRuleMapping { ActionId = "KHAI_TRUONG", RuleCode = "TAM_SAT_THANG", IsMandatory = false, GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Month, Priority = 3 },
                new ActionRuleMapping { ActionId = "KHAI_TRUONG", RuleCode = "TAM_NUONG",     IsMandatory = false, GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Month, Priority = 4 },

                // SUA_NHA Month rules
                new ActionRuleMapping { ActionId = "SUA_NHA",     RuleCode = "TAM_SAT_THANG", IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Month, Priority = 1 },

                // NHAP_TRACH Month rules - Moving in
                new ActionRuleMapping { ActionId = "NHAP_TRACH",  RuleCode = "XUNG_TUOI",     IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Month, Priority = 1 },
                new ActionRuleMapping { ActionId = "NHAP_TRACH",  RuleCode = "TAM_SAT_THANG", IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Month, Priority = 2 },

                // AN_TANG Month rules - Burial is extremely sensitive
                // NGUYET_KY and TAM_NUONG are mandatory (critical taboo days that cannot be waived)
                // XUNG_TUOI and TAM_SAT_THANG are optional (can be managed/waived)
                new ActionRuleMapping { ActionId = "AN_TANG",     RuleCode = "NGUYET_KY",     IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Month, Priority = 1 },
                new ActionRuleMapping { ActionId = "AN_TANG",     RuleCode = "TAM_NUONG",     IsMandatory = true,  GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Month, Priority = 1 },
                new ActionRuleMapping { ActionId = "AN_TANG",     RuleCode = "XUNG_TUOI",     IsMandatory = false, GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Month, Priority = 2 },
                new ActionRuleMapping { ActionId = "AN_TANG",     RuleCode = "TAM_SAT_THANG", IsMandatory = false, GenderScope = GenderApplyScope.Both,       Tier = RuleTier.Month, Priority = 2 },

                // ============================================================
                // DAY TIER RULES (Tier = RuleTier.Day)
                // ============================================================
                new ActionRuleMapping { ActionId = "XAY_NHA",     RuleCode = "XungTuoiNgay", IsMandatory = true,  GenderScope = GenderApplyScope.Both, Tier = RuleTier.Day, Priority = 1 },
                new ActionRuleMapping { ActionId = "XAY_NHA",     RuleCode = "NgayTrungTang", IsMandatory = true, GenderScope = GenderApplyScope.Both, Tier = RuleTier.Day, Priority = 2 },
                new ActionRuleMapping { ActionId = "XAY_NHA",     RuleCode = "TruongXau",    IsMandatory = false, GenderScope = GenderApplyScope.Both, Tier = RuleTier.Day, Priority = 3 },
                new ActionRuleMapping { ActionId = "CUOI_HOI",    RuleCode = "XungTuoiNgay", IsMandatory = true,  GenderScope = GenderApplyScope.Both, Tier = RuleTier.Day, Priority = 1 },
                new ActionRuleMapping { ActionId = "CUOI_HOI",    RuleCode = "DuongCongKy",  IsMandatory = true,  GenderScope = GenderApplyScope.Both, Tier = RuleTier.Day, Priority = 2 },
                new ActionRuleMapping { ActionId = "KHAI_TRUONG", RuleCode = "XungTuoiNgay", IsMandatory = true,  GenderScope = GenderApplyScope.Both, Tier = RuleTier.Day, Priority = 1 },
                new ActionRuleMapping { ActionId = "KHAI_TRUONG", RuleCode = "HacDao",       IsMandatory = true,  GenderScope = GenderApplyScope.Both, Tier = RuleTier.Day, Priority = 2 },
                new ActionRuleMapping { ActionId = "KHAI_TRUONG", RuleCode = "TrucBinh",     IsMandatory = false, GenderScope = GenderApplyScope.Both, Tier = RuleTier.Day, Priority = 3 },
                new ActionRuleMapping { ActionId = "MUA_VANG",    RuleCode = "XungTuoiNgay", IsMandatory = true,  GenderScope = GenderApplyScope.Both, Tier = RuleTier.Day, Priority = 1 },
                new ActionRuleMapping { ActionId = "MUA_VANG",    RuleCode = "HoangDao",     IsMandatory = false, GenderScope = GenderApplyScope.Both, Tier = RuleTier.Day, Priority = 2 },
                new ActionRuleMapping { ActionId = "CU_HUONG",     RuleCode = "XungTuoiNgay", IsMandatory = true,  GenderScope = GenderApplyScope.Both, Tier = RuleTier.Day, Priority = 1 },
                new ActionRuleMapping { ActionId = "CHUA_BENH",   RuleCode = "XungTuoiNgay", IsMandatory = true,  GenderScope = GenderApplyScope.Both, Tier = RuleTier.Day, Priority = 1 },
                new ActionRuleMapping { ActionId = "KHAI_TRUONG", RuleCode = "TruongXau",    IsMandatory = false, GenderScope = GenderApplyScope.Both, Tier = RuleTier.Day, Priority = 4 }
            );
        }

        await db.SaveChangesAsync();
    }
}
