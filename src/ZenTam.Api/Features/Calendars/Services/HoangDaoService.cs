using ZenTam.Api.Features.Calendars.Models;

namespace ZenTam.Api.Features.Calendars.Services;

public class HoangDaoService(IDayContextService dayContextService) : IHoangDaoService
{
    public HoangDaoInfo GetHoangDao(DateTime solarDate)
        => dayContextService.GetHoangDao(solarDate);

    public HoangDaoResponse GetHoangDaoResponse(DateTime solarDate)
    {
        var info = GetHoangDao(solarDate);
        var dayContext = dayContextService.GetDayContext(solarDate);
        return new HoangDaoResponse(
            SolarDate: solarDate,
            CanChiNgay: dayContext.CanChiNgay,
            IsHoangDao: info.IsHoangDao,
            HoangDaoHours: info.HoangDaoHours,
            HacDaoHours: info.HacDaoHours,
            TopHours: info.TopHours
        );
    }
}
