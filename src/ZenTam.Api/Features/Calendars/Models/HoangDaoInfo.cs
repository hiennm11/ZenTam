namespace ZenTam.Api.Features.Calendars.Models;

public record HoangDaoInfo(
    bool IsHoangDao,
    List<string> HoangDaoHours,   // 6 good hours: ["Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi"]
    List<string> HacDaoHours,     // 6 bad hours: ["Dần", "Mão", "Thìn", "Tỵ", "Thân", "Tuất"]
    List<string> TopHours         // Top 3 from HoangDaoHours
);
