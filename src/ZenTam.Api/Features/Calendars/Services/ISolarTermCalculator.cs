namespace ZenTam.Api.Features.Calendars.Services;

public interface ISolarTermCalculator
{
    /// <summary>
    /// Returns solar term index 0–23 where 0=Lập Xuân, 23=Đại Hàn.
    /// </summary>
    int GetSolarTermIndex(DateTime date);

    /// <summary>
    /// Returns the solar term name in Vietnamese.
    /// </summary>
    string GetSolarTermName(DateTime date);

    /// <summary>
    /// Returns solar month 1–12 (tiết khí month, NOT lunar month).
    /// Month 1 = Lập Xuân through Kinh Trập
    /// Month 2 = Kinh Trập through Thanh Minh
    /// etc.
    /// </summary>
    int GetSolarMonth(DateTime date);
}
