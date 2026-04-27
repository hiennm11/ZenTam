namespace ZenTam.Api.Features.Calendars.Models;

public record DayContext(
    DateTime SolarDate,
    string CanChiNgay,           // "Giáp Tý"
    string CanChiThang,          // "Bính Dần"
    int TrucIndex,               // 0-11 (Kiến→Bế)
    string TrucName,
    NhiThapBatTuInfo NhiThapBatTu,
    HoangDaoInfo HoangDao,
    SatChuInfo SatChu,
    ThuTuInfo ThuTu,
    bool IsNgayKy               // lunarDay in {5, 14, 23}
);
