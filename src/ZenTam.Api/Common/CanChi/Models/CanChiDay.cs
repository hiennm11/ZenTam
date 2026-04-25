namespace ZenTam.Api.Common.CanChi.Models;

/// <summary>
/// Can Chi for Day (Ngày).
/// Lookup: (jdn - 2444235) % 60 → index into 60-value table.
/// </summary>
/// <param name="Can">Heavenly Stem (Thiên Can)</param>
/// <param name="Chi">Earthly Branch (Địa Chi)</param>
public record CanChiDay(string Can, string Chi);