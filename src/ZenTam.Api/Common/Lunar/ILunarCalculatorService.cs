using ZenTam.Api.Common.Domain;

namespace ZenTam.Api.Common.Lunar;

public interface ILunarCalculatorService
{
    /// <summary>Returns the lunar year corresponding to the given solar date.</summary>
    int GetLunarYear(DateTime solarDate);

    /// <summary>Converts a solar date to a full <see cref="LunarDateContext"/>.</summary>
    LunarDateContext Convert(DateTime solarDate);

    /// <summary>
    /// Computes the Julian Day Number for a given solar date.
    /// </summary>
    int GetJulianDayNumber(int year, int month, int day);

    /// <summary>
    /// Gets the JDN of the first day (mùng 1 tháng giêng) of a lunar year.
    /// </summary>
    int GetLunarNewYearJdn(int lunarYear);
}
