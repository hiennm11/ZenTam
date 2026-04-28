namespace ZenTam.Api.Common.Domain;

public enum Gender { Male = 0, Female = 1 }

public enum GenderApplyScope
{
    Both       = 0,   // Áp dụng cho cả Nam và Nữ
    MaleOnly   = 1,   // Chỉ Nam
    FemaleOnly = 2    // Chỉ Nữ
}

public enum RuleTier
{
    Year  = 0,   // Tầng Năm: KimLau, HoangOc, TamTai, ThaiTue
    Month = 1,   // Tầng Tháng: NguyetKy, TamNuong...
    Day   = 2,   // Tầng Ngày: XungTuoiNgay, HoangDao...
    All   = 3    // Áp dụng mọi tầng
}
