namespace ZenTam.Api.Features.EvaluateSpiritualAction.Models;

public record DayScoreResult(
    DateTime SolarDate,
    string LunarDateText,
    string CanChiNgay,
    int TrucIndex,
    string TrucName,
    int TuIndex,
    string TuName,
    bool IsHoangDao,
    bool IsSatChu,
    bool IsThuTu,
    bool IsNgayKy,
    bool IsXungTuoi,
    int Score,
    int MaxScore,
    List<string> Reasons
);