namespace ZenTam.Api.Features.Calendars.Models;

public record NhiThapBatTuInfo(
    int Index,                    // 0-27
    string Name,                   // "Côn", "Đẩu", etc.
    TuClassification Classification // Kiettu, Binhtu, Hungtu
);
