using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.Lunar.Models;

namespace ZenTam.Api.Common.Lunar;

public sealed class SolarTermCalculator : ISolarTermCalculator
{
    private const double TimeZoneOffset = 7.0; // UTC+7 Vietnam Standard Time
    private const double Rad = Math.PI / 180.0;

    // 24 Solar Terms (Tiết Khí) with Vietnamese name, Chinese name, and ecliptic longitude in degrees
    private static readonly (string Name, string ChineseName, double Degrees)[] SolarTerms =
    [
        ("Xuân Phân",   "春分",   0),
        ("Thanh Minh", "清明",  15),
        ("Cốc Vũ",     "谷雨",  30),
        ("Lập Hạ",     "立夏",  45),
        ("Tiểu Mãn",   "小满",  60),
        ("Mang Chủng", "芒种",  75),
        ("Hạ Chí",     "夏至",  90),
        ("Tiểu Thử",   "小暑", 105),
        ("Đại Thử",    "大暑", 120),
        ("Lập Thu",    "立秋", 135),
        ("Xử Thử",     "处暑", 150),
        ("Bạch Lộ",    "白露", 165),
        ("Thu Phân",   "秋分", 180),
        ("Hàn Lộ",     "寒露", 195),
        ("Sương Giáng","霜降", 210),
        ("Lập Đông",   "立冬", 225),
        ("Tiểu Tuyết", "小雪", 240),
        ("Đại Tuyết",  "大雪", 255),
        ("Đông Chí",   "冬至", 270),
        ("Tiểu Hàn",   "小寒", 285),
        ("Đại Hàn",    "大寒", 300),
        ("Lập Xuân",   "立春", 315),
        ("Vũ Thủy",    "雨水", 330),
        ("Kinh Trập",  "惊蛰", 345),
    ];

    /// <summary>
    /// Returns all 24 solar terms for the given lunar year.
    /// </summary>
    public IReadOnlyList<SolarTermResult> GetSolarTerms(int year)
    {
        var results = new List<SolarTermResult>(24);

        // Get JDN of lunar new year (Tet) as anchor point
        int tetJdn = GetLunarNewYearJdn(year);

        for (int i = 0; i < SolarTerms.Length; i++)
        {
            var (name, chineseName, degrees) = SolarTerms[i];

            // Estimate JDN: each solar term is roughly 15 days apart, start ~30 days before first term
            // First term (Xuân Phân at 0°) occurs around March 20, about 60 days after Jan 1
            int baseJdn = tetJdn - 30 + (i * 15);

            // Find the precise JDN where sun longitude crosses this degree threshold
            int jdn = FindSolarTermJdn(baseJdn, degrees);

            // Convert JDN to DateTime (UTC+7)
            var date = JdnToDate(jdn);

            // Determine Gio Bat Dau (starting hour's Chi) for this solar term
            string gioBatDau = GetGioBatDau(jdn);

            results.Add(new SolarTermResult
            {
                Name = name,
                ChineseName = chineseName,
                SolarDay = date.Day,
                SolarMonth = date.Month,
                SolarYear = date.Year,
                GioBatDau = gioBatDau
            });
        }

        return results;
    }

    /// <summary>
    /// Gets a specific solar term by name for the given year.
    /// </summary>
    public SolarTermResult GetSolarTerm(string termName, int year)
    {
        var terms = GetSolarTerms(year);
        return terms.First(t => t.Name.Equals(termName, StringComparison.Ordinal));
    }

    /// <summary>
    /// Computes the solar longitude in radians for a given Julian Day Number.
    /// Meeus Chapter 7 algorithm.
    /// </summary>
    public double GetSunLongitude(double jdn)
    {
        double T  = (jdn - 2451545.0) / 36525.0;
        double T2 = T * T;
        double M  = 357.52910 + 35999.05030 * T - 0.0001559 * T2 - 0.00000048 * T * T2;
        double L0 = 280.46645 + 36000.76983 * T + 0.0003032 * T2;
        double DL = (1.9146 - 0.004817 * T - 0.000014 * T2) * Math.Sin(Rad * M)
                  + (0.019993 - 0.000101 * T) * Math.Sin(Rad * 2.0 * M)
                  + 0.00029 * Math.Sin(Rad * 3.0 * M);
        double L = (L0 + DL) * Rad;
        L -= Math.PI * 2.0 * Math.Floor(L / (Math.PI * 2.0));
        return L;
    }

    /// <summary>
    /// Gets the solar longitude in degrees (0-360).
    /// </summary>
    public double GetTrueLongitude(double jdn)
    {
        return GetSunLongitude(jdn) / Rad;
    }

    /// <summary>
    /// Finds the JDN where the sun's ecliptic longitude crosses the specified degree threshold.
    /// Uses binary search for precision.
    /// </summary>
    private int FindSolarTermJdn(int startJdn, double targetDegrees)
    {
        int low = startJdn - 5;
        int high = startJdn + 5;

        // Normalize target to 0-360
        double targetRad = targetDegrees * Rad;
        double targetNormalized = targetDegrees;

        for (int i = 0; i < 10; i++)
        {
            int mid = (low + high) / 2;
            double midLon = GetTrueLongitude(mid);

            // Normalize to 0-360 for comparison
            double midNormalized = ((midLon % 360) + 360) % 360;

            // Check if we crossed the target
            double lowLon = GetTrueLongitude(low);
            double lowNormalized = ((lowLon % 360) + 360) % 360;
            double highLon = GetTrueLongitude(high);
            double highNormalized = ((highLon % 360) + 360) % 360;

            // Check if crossing occurred between low and mid
            if (CrossesBoundary(lowNormalized, midNormalized, targetNormalized))
            {
                high = mid;
            }
            // Check if crossing occurred between mid and high
            else if (CrossesBoundary(midNormalized, highNormalized, targetNormalized))
            {
                low = mid;
            }
            else
            {
                // No clear crossing, narrow search range
                if (midNormalized < targetNormalized)
                    low = mid;
                else
                    high = mid;
            }
        }

        // Return the integer JDN closest to the crossing
        return (low + high) / 2;
    }

    /// <summary>
    /// Checks if the interval [a, b] crosses the target boundary.
    /// </summary>
    private static bool CrossesBoundary(double a, double b, double target)
    {
        a = ((a % 360) + 360) % 360;
        b = ((b % 360) + 360) % 360;
        target = ((target % 360) + 360) % 360;

        if (a < b)
        {
            return a <= target && target < b;
        }
        else
        {
            // Wrapped around 360
            return a <= target || target < b;
        }
    }

    /// <summary>
    /// Gets the lunar new year JDN for a given lunar year.
    /// Uses the AmLichCalculator's method via its public interface.
    /// </summary>
    private static int GetLunarNewYearJdn(int lunarYear)
    {
        var calculator = new AmLichCalculator();
        return calculator.GetLunarNewYearJdn(lunarYear);
    }

    /// <summary>
    /// Converts a Julian Day Number to a DateTime in UTC+7.
    /// </summary>
    private static DateTime JdnToDate(int jdn)
    {
        // Algorithm from Meeus, Chapter 7
        int Z  = jdn;
        int A  = Z;
        if (Z >= 2299161)
        {
            int alpha = (int)Math.Floor((Z - 1867216.25) / 36524.25);
            A = Z + 1 + alpha - alpha / 4;
        }
        int B  = A + 1524;
        int C  = (int)Math.Floor((B - 122.1) / 365.25);
        int D  = (int)Math.Floor(365.25 * C);
        int E  = (int)Math.Floor((B - D) / 30.6001);

        int day   = B - D - (int)Math.Floor(30.6001 * E);
        int month = E < 14 ? E - 1 : E - 13;
        int year  = month > 2 ? C - 4716 : C - 4715;

        return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
    }

    /// <summary>
    /// Determines the Gio Bat Dau (starting hour's Chi) for a given solar term.
    /// Based on the solar term's position in the annual cycle.
    /// </summary>
    private static string GetGioBatDau(int jdn)
    {
        // The Gio Bat Dau for a solar term is determined by the day's Chi
        // which cycles through the 12 Earthly Branches
        // Each day of the solar term period has a specific Chi

        var calculator = new CanChiCalculator();
        var canChi = calculator.GetCanChiNgay(jdn);
        return canChi.Chi;
    }
}