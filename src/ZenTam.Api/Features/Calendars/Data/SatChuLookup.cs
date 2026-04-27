namespace ZenTam.Api.Features.Calendars.Data;

public static class SatChuLookup
{
    // lunarMonth 1-12 → satChuLunarDay
    private static readonly int[] Table =
        /*  1   2   3   4   5   6   7   8   9  10  11  12 */
        [   9,  2, 27,  4, 21, 17, 10, 25,  3, 20, 23, 27 ];

    public static int GetSatChuDay(int lunarMonth) => Table[lunarMonth - 1];

    public static bool IsSatChu(int lunarMonth, int lunarDay)
        => GetSatChuDay(lunarMonth) == lunarDay;
}
