namespace ZenTam.Api.Domain.Rules.MonthlyRuleEngine.Models;

public enum DayVerdict
{
    Binh = 0,       // Neutral
    Cat = 1,        // Auspicious
    DaiCat = 2,     // Very Auspicious
    Hung = 3,       // Inauspicious
    DaiHung = 4,    // Highly Inauspicious
    TuVong = 5      // Death/Extreme
}