using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Features.Calendars.Data;
using ZenTam.Api.Features.Calendars.Models;

namespace ZenTam.Api.Features.Calendars.Services;

public class DayContextService(
    ICanChiCalculator canChiCalculator,
    ILunarCalculatorService lunarCalculator,
    ISolarTermCalculator solarTermCalculator
) : IDayContextService
{
    // 10 Can names
    private static readonly string[] CanNames =
        ["Giáp", "Ất", "Bính", "Đinh", "Mậu", "Kỷ", "Canh", "Tân", "Nhâm", "Quý"];

    // 12 Chi names
    private static readonly string[] ChiNames =
        ["Tý", "Sửu", "Dần", "Mão", "Thìn", "Tỵ", "Ngọ", "Mùi", "Thân", "Dậu", "Tuất", "Hợi"];

    // Nhị Thập Bát Tú names
    private static readonly string[] TuNames =
    [
        "Côn", "Đẩu", "Nữ", "Bộc", "Tham", "Chu",
        "Xà", "Tinh", "Trương", "Dực", "Chẩn", "Vĩ",
        "Mão", "Tất", "Mão", "Chu", "Sâm", "Tỉ",
        "Thư", "Vị", "Trủy", "Nguy", "Dương",
        "Quân", "Lâu", "Vệ", "Cư", "Mỹ"
    ];

    public DayContext GetDayContext(DateTime solarDate)
    {
        var jdn = lunarCalculator.GetJulianDayNumber(solarDate.Year, solarDate.Month, solarDate.Day);
        var lunar = lunarCalculator.Convert(solarDate);
        var canChiNgay = canChiCalculator.GetCanChiNgay(jdn);
        var canChiThang = canChiCalculator.GetCanChiThang(lunar.LunarYear, lunar.LunarMonth, lunar.IsLeap);
        var truc = canChiCalculator.GetThapNhiTruc(solarDate);

        return new DayContext(
            SolarDate: solarDate,
            CanChiNgay: $"{canChiNgay.Can} {canChiNgay.Chi}",
            CanChiThang: $"{canChiThang.Can} {canChiThang.Chi}",
            TrucIndex: truc,
            TrucName: canChiCalculator.GetTrucName(truc),
            NhiThapBatTu: GetNhiThapBatTu(solarDate),
            HoangDao: GetHoangDao(solarDate),
            SatChu: GetSatChu(solarDate),
            ThuTu: GetThuTu(solarDate),
            IsNgayKy: lunar.LunarDay is 5 or 14 or 23
        );
    }

    public NhiThapBatTuInfo GetNhiThapBatTu(DateTime solarDate)
    {
        int jdn = lunarCalculator.GetJulianDayNumber(solarDate.Year, solarDate.Month, solarDate.Day);
        int tuIndex = canChiCalculator.GetNhiThapBatTu(jdn);
        return new NhiThapBatTuInfo(
            tuIndex,
            TuNames[tuIndex],
            GetClassification(tuIndex)
        );
    }

    public HoangDaoInfo GetHoangDao(DateTime solarDate)
    {
        int jdn = lunarCalculator.GetJulianDayNumber(solarDate.Year, solarDate.Month, solarDate.Day);
        var canChi = canChiCalculator.GetCanChiNgay(jdn);
        int canIndex = Array.IndexOf(CanNames, canChi.Can);
        int chiIndex = Array.IndexOf(ChiNames, canChi.Chi);
        return HoangDaoLookup.GetHoangDao(canIndex, chiIndex);
    }

    public SatChuInfo GetSatChu(DateTime solarDate)
    {
        var lunar = lunarCalculator.Convert(solarDate);
        int satChuDay = SatChuLookup.GetSatChuDay(lunar.LunarMonth);
        return new SatChuInfo(
            lunar.LunarDay == satChuDay,
            satChuDay
        );
    }

    public ThuTuInfo GetThuTu(DateTime solarDate)
    {
        var lunar = lunarCalculator.Convert(solarDate);
        int jdn = lunarCalculator.GetJulianDayNumber(solarDate.Year, solarDate.Month, solarDate.Day);
        var canChi = canChiCalculator.GetCanChiNgay(jdn);
        int chiIndex = Array.IndexOf(ChiNames, canChi.Chi);
        bool isThuTu = ThuTuLookup.IsThuTu(lunar.LunarMonth, chiIndex);
        int[] forbidden = isThuTu ? ThuTuLookup.GetForbiddenChi(lunar.LunarMonth) : [];
        return new ThuTuInfo(isThuTu, forbidden);
    }

    private static TuClassification GetClassification(int index) => index switch
    {
        >= 0 and <= 5 => TuClassification.Kiettu,
        >= 12 and <= 22 => TuClassification.Hungtu,
        _ => TuClassification.Binhtu
    };
}
