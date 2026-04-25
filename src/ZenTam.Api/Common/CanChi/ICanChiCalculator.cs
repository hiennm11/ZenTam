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
    /// <param name="lunarYear">Lunar year</param>
    /// <returns>CanChiYear with Can and Chi values</returns>
    CanChiYear GetCanChiNam(int lunarYear);

    /// <summary>
    /// Gets Can Chi for lunar month.
    /// Can depends on year's Can (1st month = Giáp for 甲/乙 years, Bính for 丙/丁, etc.)
    /// Chi cycles: Tý→Hợi (month 1→12)
    /// </summary>
    /// <param name="lunarYear">Lunar year</param>
    /// <param name="lunarMonth">Lunar month (1-12)</param>
    /// <param name="isLeapMonth">True if this is a leap month</param>
    /// <returns>CanChiMonth with Can and Chi values</returns>
    CanChiMonth GetCanChiThang(int lunarYear, int lunarMonth, bool isLeapMonth);

    /// <summary>
    /// Gets Can Chi for day from JDN.
    /// Lookup: (jdn - 2444235) % 60 → index into 60-value table.
    /// </summary>
    /// <param name="jdn">Julian Day Number</param>
    /// <returns>CanChiDay with Can and Chi values</returns>
    CanChiDay GetCanChiNgay(int jdn);

    /// <summary>
    /// Gets Can Chi for hour.
    /// gioBatDau: 23=Tý, 1=Sửu, 3=Dần... (23 means start at Tý hour)
    /// </summary>
    /// <param name="jdn">Julian Day Number</param>
    /// <param name="gioBatDau">Starting hour (23, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21)</param>
    /// <returns>CanChiHour with Can and Chi values</returns>
    CanChiHour GetCanChiGio(int jdn, int gioBatDau);

    /// <summary>
    /// Thập Nhị Trực (12 Earthly Branches for days).
    /// Formula: (jdn + 1) % 12 → index 0–11
    /// 0:Kiến, 1:Trừ, 2:Mãn, 3:Bình, 4:Định, 5:Chấp,
    /// 6:Phá, 7:Nguy, 8:Thành, 9:Thu, 10:Khai, 11:Bế
    /// </summary>
    /// <param name="jdn">Julian Day Number</param>
    /// <returns>Index 0-11 into ThapNhiTrucNames array</returns>
    int GetThapNhiTruc(int jdn);

    /// <summary>
    /// Nhị Thập Bát Tú (28 Lunar Mansions).
    /// Formula: jdn % 28 → index 0–27
    /// </summary>
    /// <param name="jdn">Julian Day Number</param>
    /// <returns>Index 0-27 into NhiThapBatTuNames array</returns>
    int GetNhiThapBatTu(int jdn);
}