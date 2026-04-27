namespace ZenTam.Api.Features.Calendars.Models;

public record SatChuInfo(
    bool IsSatChu,     // true if today is Sát Chủ
    int LunarDay       // The Sát Chủ lunar day for this month (or -1)
);
