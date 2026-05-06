using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.Lunar.Models;

namespace ZenTam.Api.Common.Lunar;

public sealed class LunarCalendarService : ILunarCalendarService
{
    private readonly AmLichCalculator _amLich;
    private readonly CanChiCalculator _canChi;
    private readonly ISolarTermCalculator _solarTerm;

    // Chi-to-GioHoangDao mapping (4 lucky hours per Chi day)
    // Based on traditional Vietnamese astrology
    private static readonly Dictionary<string, string[]> GioHoangDaoTable = new()
    {
        ["Tý"] = ["Tý", "Sửu", "Ngọ", "Mùi"],
        ["Sửu"] = ["Sửu", "Dần", "Mùi", "Thân"],
        ["Dần"] = ["Dần", "Mão", "Thân", "Dậu"],
        ["Mão"] = ["Mão", "Thìn", "Dậu", "Tuất"],
        ["Thìn"] = ["Thìn", "Tỵ", "Tuất", "Hợi"],
        ["Tỵ"] = ["Tỵ", "Ngọ", "Hợi", "Tý"],
        ["Ngọ"] = ["Ngọ", "Mùi", "Tý", "Sửu"],
        ["Mùi"] = ["Mùi", "Thân", "Sửu", "Dần"],
        ["Thân"] = ["Thân", "Dậu", "Dần", "Mão"],
        ["Dậu"] = ["Dậu", "Tuất", "Mão", "Thìn"],
        ["Tuất"] = ["Tuất", "Hợi", "Thìn", "Tỵ"],
        ["Hợi"] = ["Hợi", "Tý", "Tỵ", "Ngọ"],
    };

    public LunarCalendarService()
    {
        _amLich = new AmLichCalculator();
        _canChi = new CanChiCalculator();
        _solarTerm = new SolarTermCalculator();
    }

    public LunarCalendarService(AmLichCalculator amLich, CanChiCalculator canChi, ISolarTermCalculator solarTerm)
    {
        _amLich = amLich ?? new AmLichCalculator();
        _canChi = canChi ?? new CanChiCalculator();
        _solarTerm = solarTerm ?? new SolarTermCalculator();
    }

    /// <inheritdoc/>
    public LunarDateResult ConvertToLunar(int solarYear, int solarMonth, int solarDay)
    {
        var date = new DateTime(solarYear, solarMonth, solarDay, 0, 0, 0, DateTimeKind.Unspecified);
        var lunar = _amLich.Convert(date);
        var jdn = _amLich.GetJulianDayNumber(solarYear, solarMonth, solarDay);
        var canChi = _canChi.GetCanChiNgay(jdn);
        var gioHoangDao = GetGioHoangDaoFromChi(canChi.Chi);

        return new LunarDateResult
        {
            LunarYear = lunar.LunarYear,
            LunarMonth = lunar.LunarMonth,
            LunarDay = lunar.LunarDay,
            IsLeapMonth = lunar.IsLeap,
            GioHoangDao = gioHoangDao
        };
    }

    /// <inheritdoc/>
    public TetResult GetTetDate(int solarYear)
    {
        var jdn = _amLich.GetLunarNewYearJdn(solarYear);
        var date = JdnToDate(jdn);
        var lunar = _amLich.Convert(date);

        return new TetResult
        {
            SolarDay = date.Day,
            SolarMonth = date.Month,
            SolarYear = date.Year,
            LunarDay = 1,
            LunarMonth = 1,
            LunarYear = lunar.LunarYear
        };
    }

    /// <inheritdoc/>
    public string GetGioHoangDao(int solarYear, int solarMonth, int solarDay)
    {
        var jdn = _amLich.GetJulianDayNumber(solarYear, solarMonth, solarDay);
        var canChi = _canChi.GetCanChiNgay(jdn);
        return GetGioHoangDaoFromChi(canChi.Chi);
    }

    /// <summary>
    /// Derives the Gio Hoang Dao (lucky hours) from the Earth's Branch (Chi) of the day.
    /// </summary>
    private static string GetGioHoangDaoFromChi(string chi)
    {
        if (GioHoangDaoTable.TryGetValue(chi, out var gios))
        {
            return string.Join(", ", gios);
        }
        return string.Empty;
    }

    /// <summary>
    /// Converts a Julian Day Number to a DateTime in UTC+7.
    /// </summary>
    private static DateTime JdnToDate(int jdn)
    {
        int Z = jdn;
        int A = Z;
        if (Z >= 2299161)
        {
            int alpha = (int)Math.Floor((Z - 1867216.25) / 36524.25);
            A = Z + 1 + alpha - alpha / 4;
        }
        int B = A + 1524;
        int C = (int)Math.Floor((B - 122.1) / 365.25);
        int D = (int)Math.Floor(365.25 * C);
        int E = (int)Math.Floor((B - D) / 30.6001);

        int day = B - D - (int)Math.Floor(30.6001 * E);
        int month = E < 14 ? E - 1 : E - 13;
        int year = month > 2 ? C - 4716 : C - 4715;

        return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
    }
}