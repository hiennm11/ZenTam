using ZenTam.Api.Common.CanChi.Models;

namespace ZenTam.Api.Common.CanChi;

/// <summary>
/// Can Chi (Stem-Branch) Calculator.
/// Base: JDN for day calculations, formulas for year/month/hour.
/// </summary>
public interface ICanChiCalculator
{
    /// <summary>
    /// Gets Can Chi for lunar year.
    /// Formula: Can = (lunarYear + 6) % 10, Chi = (lunarYear + 8) % 12
    /// </summary>
    CanChiYear GetCanChiNam(int lunarYear);

    /// <summary>
    /// Gets Can Chi for lunar month.
    /// Can depends on year's Can (1st month = Giáp for 甲/乙 years, Bính for 丙/丁, etc.)
    /// Chi cycles: Tý→Hợi (month 1→12)
    /// </summary>
    CanChiMonth GetCanChiThang(int lunarYear, int lunarMonth, bool isLeapMonth);

    /// <summary>
    /// Gets Can Chi for day from JDN.
    /// Lookup: (jdn - 2444235) % 60 → index into 60-value table.
    /// </summary>
    CanChiDay GetCanChiNgay(int jdn);

    /// <summary>
    /// Computes JDN for a solar date.
    /// </summary>
    int GetJulianDayNumber(int year, int month, int day);

    /// <summary>
    /// Gets Can Chi for hour.
    /// gioBatDau: 23=Tý, 1=Sửu, 3=Dần... (23 means start at Tý hour)
    /// </summary>
    CanChiHour GetCanChiGio(int jdn, int gioBatDau);

    /// <summary>
    /// Nhị Thập Bát Tú (28 Lunar Mansions).
    /// Formula: jdn % 28 → index 0–27
    /// </summary>
    int GetNhiThapBatTu(int jdn);

    /// <summary>
    /// Trực (0–11) daily cycle.
    /// Formula: (jdn + 3) % 12
    /// </summary>
    int GetTru(int jdn);
}