using ZenTam.Api.Features.Calendars.Models;

namespace ZenTam.Api.Features.Calendars.Services;

public interface IHoangDaoService
{
    HoangDaoInfo GetHoangDao(DateTime solarDate);
    HoangDaoResponse GetHoangDaoResponse(DateTime solarDate);
}

public record HoangDaoResponse(
    DateTime SolarDate,
    string CanChiNgay,
    bool IsHoangDao,
    List<string> HoangDaoHours,
    List<string> HacDaoHours,
    List<string> TopHours
);
