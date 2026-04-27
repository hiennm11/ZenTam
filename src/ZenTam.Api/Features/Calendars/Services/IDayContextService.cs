using ZenTam.Api.Features.Calendars.Models;

namespace ZenTam.Api.Features.Calendars.Services;

public interface IDayContextService
{
    DayContext GetDayContext(DateTime solarDate);
    NhiThapBatTuInfo GetNhiThapBatTu(DateTime solarDate);
    HoangDaoInfo GetHoangDao(DateTime solarDate);
    SatChuInfo GetSatChu(DateTime solarDate);
    ThuTuInfo GetThuTu(DateTime solarDate);
}
