using ZenTam.Api.Common.Domain;

namespace ZenTam.Api.Common.Lunar;

/// <summary>
/// Vietnamese Lunar Calendar (Âm Lịch) calculator.
/// Ported from Ho Ngoc Duc's amlich.js algorithm.
/// Source: http://www.informatik.uni-leipzig.de/~duc/amlich/calrules.html
/// All astronomical methods mirror the JavaScript originals; see inline citations.
/// </summary>
public class AmLichCalculator : ILunarCalculatorService
{
    private static readonly TimeZoneInfo VietnamTimeZone = CreateVietnamTimeZone();
    private const double TimeZoneOffset = 7.0; // UTC+7

    private static TimeZoneInfo CreateVietnamTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch
        {
            return TimeZoneInfo.CreateCustomTimeZone(
                "UTC+7", TimeSpan.FromHours(7), "UTC+7", "UTC+7");
        }
    }

    /// <inheritdoc/>
    public int GetLunarYear(DateTime solarDate)
    {
        var local = TimeZoneInfo.ConvertTime(solarDate, VietnamTimeZone);
        var (_, _, lunarYear, _) = Solar2Lunar(local);
        return lunarYear;
    }

    /// <inheritdoc/>
    public LunarDateContext Convert(DateTime solarDate)
    {
        var local = TimeZoneInfo.ConvertTime(solarDate, VietnamTimeZone);
        var (lunarDay, lunarMonth, lunarYear, isLeap) = Solar2Lunar(local);
        return new LunarDateContext
        {
            LunarDay   = lunarDay,
            LunarMonth = lunarMonth,
            LunarYear  = lunarYear,
            IsLeap     = isLeap
        };
    }

    /// <inheritdoc/>
    public int GetJulianDayNumber(int year, int month, int day)
        => JulianDayNumber(day, month, year);

    /// <inheritdoc/>
    public double GetSunLongitude(int jdn) => SunLongitude(jdn);

    /// <inheritdoc/>
    public int GetLunarNewYearJdn(int lunarYear)
    {
        // Find a solar date that falls on mùng 1 tháng giêng of the given lunar year
        // Strategy: search around January-February of the solar year
        var searchYear = lunarYear;
        
        // First try around January 20-February 20 of the solar year matching lunar year
        for (int month = 1; month <= 2; month++)
        {
            for (int day = 20; day <= 28; day++)
            {
                var solar = new DateTime(searchYear, month, day);
                var lunar = Convert(solar);
                if (lunar.LunarYear == lunarYear && lunar.LunarMonth == 1 && lunar.LunarDay == 1)
                {
                    return GetJulianDayNumber(searchYear, month, day);
                }
            }
        }
        
        // Fallback: binary search in January-February range
        int low = 20, high = 59; // days in Jan-Feb
        while (low <= high)
        {
            int mid = (low + high) / 2;
            int m = mid <= 31 ? 1 : 2;
            int d = mid <= 31 ? mid : mid - 31;
            var solar = new DateTime(searchYear, m, d);
            var lunar = Convert(solar);
            
            if (lunar.LunarYear < lunarYear)
                low = mid + 1;
            else if (lunar.LunarYear > lunarYear)
                high = mid - 1;
            else if (lunar.LunarMonth < 1)
                low = mid + 1;
            else if (lunar.LunarMonth > 1)
                high = mid - 1;
            else if (lunar.LunarDay < 1)
                low = mid + 1;
            else if (lunar.LunarDay > 1)
                high = mid - 1;
            else
                return GetJulianDayNumber(searchYear, m, d);
        }
        
        // Fallback calculation: approximate Tết date
        // Lunar new year is between Jan 21 and Feb 20
        for (int offset = 0; offset <= 30; offset++)
        {
            var candidate = new DateTime(searchYear, 1, 21).AddDays(offset);
            var lunar = Convert(candidate);
            if (lunar.LunarYear == lunarYear && lunar.LunarMonth == 1 && lunar.LunarDay == 1)
            {
                return GetJulianDayNumber(searchYear, 1, 21 + offset);
            }
        }
        
        throw new InvalidOperationException($"Cannot find lunar new year JDN for lunar year {lunarYear}");
    }

    // ─── Core conversion ────────────────────────────────────────────────────────

    /// <summary>
    /// Converts a solar date (already in UTC+7) to its Vietnamese lunar components.
    /// Returns (lunarDay, lunarMonth, lunarYear, isLeapMonth).
    /// Ported from amlich.js: convertSolar2Lunar().
    /// </summary>
    private static (int lunarDay, int lunarMonth, int lunarYear, bool isLeap) Solar2Lunar(
        DateTime solar)
    {
        int dd = solar.Day, mm = solar.Month, yy = solar.Year;

        int dayNumber  = JulianDayNumber(dd, mm, yy);
        int k          = (int)Math.Floor((dayNumber - 2415021.076998695) / 29.530588853);
        int monthStart = GetNewMoonDay(k + 1, TimeZoneOffset);
        if (monthStart > dayNumber)
            monthStart = GetNewMoonDay(k, TimeZoneOffset);

        int a11 = GetLunarMonth11(yy, TimeZoneOffset);
        int b11 = a11;
        int lunarYear;

        if (a11 >= monthStart)
        {
            lunarYear = yy;
            a11       = GetLunarMonth11(yy - 1, TimeZoneOffset);
        }
        else
        {
            lunarYear = yy + 1;
            b11       = GetLunarMonth11(yy + 1, TimeZoneOffset);
        }

        int  lunarDay  = dayNumber - monthStart + 1;
        int  diff      = (int)Math.Floor((double)(monthStart - a11) / 29);
        bool lunarLeap = false;
        int  lunarMonth = diff + 11;

        if (b11 - a11 > 365)
        {
            int leapMonthDiff = GetLeapMonthOffset(a11, TimeZoneOffset);
            if (diff >= leapMonthDiff)
            {
                lunarMonth = diff + 10;
                if (diff == leapMonthDiff)
                    lunarLeap = true;
            }
        }

        if (lunarMonth > 12) lunarMonth -= 12;
        if (lunarMonth >= 11 && diff < 4) lunarYear -= 1;

        return (lunarDay, lunarMonth, lunarYear, lunarLeap);
    }

    // ─── Astronomical helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Converts a Gregorian date to its Julian Day Number (JDN).
    /// Handles both Gregorian (post 1582-10-15) and Julian calendar dates.
    /// Ported from amlich.js: jdFromDate().
    /// </summary>
    private static int JulianDayNumber(int d, int m, int y)
    {
        int a  = (14 - m) / 12;
        int yr = y + 4800 - a;
        int mo = m + 12 * a - 3;
        int jd = d + (153 * mo + 2) / 5 + 365 * yr + yr / 4 - yr / 100 + yr / 400 - 32045;
        if (jd < 2299161)
            jd = d + (153 * mo + 2) / 5 + 365 * yr + yr / 4 - 32083;
        return jd;
    }

    /// <summary>
    /// Computes the approximate solar longitude in radians for a given Julian Day.
    /// Ported from amlich.js: SunLongitude().
    /// </summary>
    private static double SunLongitude(double jdn)
    {
        double dr = Math.PI / 180;
        double T  = (jdn - 2451545.0) / 36525;
        double T2 = T * T;
        double M  = 357.52910 + 35999.05030 * T - 0.0001559 * T2 - 0.00000048 * T * T2;
        double L0 = 280.46645 + 36000.76983 * T + 0.0003032 * T2;
        double DL = (1.9146 - 0.004817 * T - 0.000014 * T2) * Math.Sin(dr * M)
                  + (0.019993 - 0.000101 * T) * Math.Sin(dr * 2 * M)
                  + 0.00029 * Math.Sin(dr * 3 * M);
        double L = (L0 + DL) * dr;
        L -= Math.PI * 2 * Math.Floor(L / (Math.PI * 2));
        return L;
    }

    /// <summary>
    /// Returns the Julian Day Number of the k-th new moon (k=0 → Jan 1900).
    /// Ported from amlich.js: getNewMoonDay().
    /// </summary>
    private static int GetNewMoonDay(int k, double timeZone)
        => (int)Math.Floor(NewMoon(k) + 0.5 + timeZone / 24);

    /// <summary>
    /// Returns the solar-longitude sector (0–11, each 30°) for a given Julian Day.
    /// Used to detect the start of each lunar month.
    /// Ported from amlich.js: getSunLongitude().
    /// </summary>
    private static int GetSunLongitudeAtNewMoon(int dayNumber, double timeZone)
        => (int)Math.Floor(SunLongitude(dayNumber - 0.5 - timeZone / 24) / Math.PI * 6);

    /// <summary>
    /// Finds the Julian Day Number of the first day of the 11th lunar month in year yy.
    /// The 11th lunar month (Tháng 11 âm) always falls near the winter solstice.
    /// Ported from amlich.js: getLunarMonth11().
    /// </summary>
    private static int GetLunarMonth11(int yy, double timeZone)
    {
        int nm      = GetNewMoonDay((int)Math.Floor((yy - 2000) * 12.3685) - 1, timeZone);
        int sunLong = GetSunLongitudeAtNewMoon(nm + 1, timeZone);
        if (sunLong >= 9)
            nm = GetNewMoonDay((int)Math.Floor((yy - 2000) * 12.3685) - 2, timeZone);
        return nm;
    }

    /// <summary>
    /// Determines which new moon in the cycle (after the 11th-month anchor) is the leap month.
    /// Returns the offset index from the anchor new moon.
    /// Ported from amlich.js: getLeapMonthOffset().
    /// </summary>
    private static int GetLeapMonthOffset(int a11, double timeZone)
    {
        int k    = (int)Math.Floor((a11 - 2415021.076998695) / 29.530588853 + 0.5);
        int last = 0;
        int i    = 1;
        int arc  = GetSunLongitudeAtNewMoon(GetNewMoonDay(k + i, timeZone), timeZone);
        do
        {
            last = arc;
            i++;
            arc = GetSunLongitudeAtNewMoon(GetNewMoonDay(k + i, timeZone), timeZone);
        } while (arc != last && i < 14);
        return i - 1;
    }

    /// <summary>
    /// Computes the precise Julian Day of the k-th new moon using lunar theory corrections.
    /// Ported from amlich.js: NewMoon().
    /// </summary>
    private static double NewMoon(int k)
    {
        double dr  = Math.PI / 180;
        double T   = k / 1236.85;
        double T2  = T * T;
        double T3  = T2 * T;
        double Jd1 = 2415020.75933 + 29.53058868 * k + 0.0001178 * T2 - 0.000000155 * T3;
        Jd1 += 0.00033 * Math.Sin((166.56 + 132.87 * T - 0.009173 * T2) * dr);

        double M   = 359.2242  + 29.10535608  * k - 0.0000333  * T2 - 0.00000347  * T3;
        double Mpr = 306.0253  + 385.81691806 * k + 0.0107306  * T2 + 0.00001236  * T3;
        double F   = 21.2964   + 390.67050646 * k - 0.0016528  * T2 - 0.00000239  * T3;

        double C1 = (0.1734 - 0.000393 * T) * Math.Sin(M   * dr) + 0.0021 * Math.Sin(2 * dr * M);
        C1 -= 0.4068 * Math.Sin(Mpr * dr) + 0.0161 * Math.Sin(2 * dr * Mpr);
        C1 -= 0.0004 * Math.Sin(3 * dr * Mpr);
        C1 += 0.0104 * Math.Sin(2 * dr * F)       - 0.0051 * Math.Sin(dr * (M + Mpr));
        C1 -= 0.0074 * Math.Sin(dr * (M - Mpr))   + 0.0004 * Math.Sin(dr * (2 * F + M));
        C1 -= 0.0004 * Math.Sin(dr * (2 * F - M)) - 0.0006 * Math.Sin(dr * (2 * F + Mpr));
        C1 += 0.0010 * Math.Sin(dr * (2 * F - Mpr)) + 0.0005 * Math.Sin(dr * (2 * Mpr + M));

        double deltat = T < -11
            ? 0.001 + 0.000839 * T + 0.0002261 * T2 - 0.00000845 * T3 - 0.000000081 * T * T3
            : -0.000278 + 0.000265 * T + 0.000262 * T2;

        return Jd1 + C1 - deltat;
    }
}
