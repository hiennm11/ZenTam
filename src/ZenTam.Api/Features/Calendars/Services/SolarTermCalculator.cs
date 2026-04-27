using ZenTam.Api.Features.Calendars.Models;

namespace ZenTam.Api.Features.Calendars.Services;

public class SolarTermCalculator(
    ZenTam.Api.Common.Lunar.ILunarCalculatorService lunarCalculator
) : ISolarTermCalculator
{
    private static readonly string[] SolarTermNames =
    [
        "Lập Xuân", "Vũ Thủy", "Kinh Trập", "Xuân Phân",
        "Thanh Minh", "Cốc Vũ", "Lập Hạ", "Tiểu Mãn",
        "Mang Chủng", "Hạ Chí", "Tiểu Thử", "Đại Thử",
        "Lập Thu", "Xử Thử", "Bạch Lộ", "Thu Phân",
        "Hàn Lộ", "Sương Giáng", "Lập Đông", "Tiểu Tuyết",
        "Đại Tuyết", "Đông Chí", "Tiểu Hàn", "Đại Hàn"
    ];

    public int GetSolarTermIndex(DateTime date)
    {
        ArgumentNullException.ThrowIfNull(date);
        
        int jdn = lunarCalculator.GetJulianDayNumber(date.Year, date.Month, date.Day);
        double sunLongRadians = lunarCalculator.GetSunLongitude(jdn);
        
        // Convert radians to degrees for proper mapping
        double sunLongDegrees = sunLongRadians * 180.0 / Math.PI;
        
        // Normalize to 0-360 range
        double normalizedDegrees = sunLongDegrees % 360.0;
        if (normalizedDegrees < 0) normalizedDegrees += 360.0;
        
        // Solar terms start at 315° (Lập Xuân). Normalize so that 315° → 0, 330° → 1, etc.
        // If degrees >= 315, subtract 315; otherwise add 45 to get the offset past the cycle
        double offsetDegrees;
        if (normalizedDegrees >= 315.0)
        {
            offsetDegrees = normalizedDegrees - 315.0;
        }
        else
        {
            offsetDegrees = normalizedDegrees + 45.0; // 360 - 315 = 45
        }
        
        // Each solar term is 15°
        int index = (int)(offsetDegrees / 15.0);
        return index;
    }

    public string GetSolarTermName(DateTime date)
    {
        ArgumentNullException.ThrowIfNull(date);
        return SolarTermNames[GetSolarTermIndex(date)];
    }

    public int GetSolarMonth(DateTime date)
    {
        ArgumentNullException.ThrowIfNull(date);
        
        int index = GetSolarTermIndex(date);
        return (index / 2) + 1; // indices 0-1 → month 1, 2-3 → month 2, etc.
    }
}
