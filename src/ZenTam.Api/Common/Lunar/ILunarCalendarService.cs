using ZenTam.Api.Common.Lunar.Models;

namespace ZenTam.Api.Common.Lunar;

public interface ILunarCalendarService
{
    LunarDateResult ConvertToLunar(int solarYear, int solarMonth, int solarDay);
    TetResult GetTetDate(int solarYear);
    string GetGioHoangDao(int solarYear, int solarMonth, int solarDay);
}