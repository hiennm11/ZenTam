namespace ZenTam.Api.Domain.Rules.MonthlyRuleEngine;

/// <summary>
/// Helper class for DayLevel determination.
/// </summary>
public static class DayLevelCalculator
{
    /// <summary>
    /// Determines the DayLevel based on lunar day and month.
    /// </summary>
    public static Models.DayLevel DetermineDayLevel(int lunarDay, int lunarMonth)
    {
        if (lunarDay == 1)
            return Models.DayLevel.NgayMung1;

        int daysInMonth = (lunarMonth == 1 || lunarMonth == 3 || lunarMonth == 5 ||
                           lunarMonth == 7 || lunarMonth == 8 || lunarMonth == 10 || lunarMonth == 12)
                         ? 30 : 29;

        if (lunarDay == daysInMonth)
            return Models.DayLevel.NgayCuoi;

        if (lunarDay % 2 == 0)
            return Models.DayLevel.NgayChan;

        return Models.DayLevel.NgayLe;
    }
}