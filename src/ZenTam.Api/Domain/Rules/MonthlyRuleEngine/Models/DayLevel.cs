namespace ZenTam.Api.Domain.Rules.MonthlyRuleEngine.Models;

public enum DayLevel
{
    NgayMung1 = 1,   // Lunar day 1
    NgayChan = 2,    // Even days: 2,4,6,8,10,12,14,16,18,20,22,24,26,28,30
    NgayLe = 3,      // Odd days: 3,5,7,9,11,13,15,17,19,21,23,25,27,29
    NgayCuoi = 4     // Last day of lunar month
}