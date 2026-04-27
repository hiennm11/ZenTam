namespace ZenTam.Api.Features.Calendars.Models;

public record ThuTuInfo(
    bool IsThuTu,      // true if today is Thọ Tử
    int[] ForbiddenChi // Array of 2 forbidden chi indices for this month (or empty)
);
