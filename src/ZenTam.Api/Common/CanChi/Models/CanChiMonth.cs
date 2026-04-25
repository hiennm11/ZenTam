namespace ZenTam.Api.Common.CanChi.Models;

/// <summary>
/// Can Chi for Month (Tháng).
/// Can depends on year's Can (1st month = Giáp for 甲/乙 years, Bính for 丙/丁, etc.)
/// Chi cycles: Tý→Hợi (month 1→12)
/// </summary>
/// <param name="Can">Heavenly Stem (Thiên Can)</param>
/// <param name="Chi">Earthly Branch (Địa Chi)</param>
public record CanChiMonth(string Can, string Chi);