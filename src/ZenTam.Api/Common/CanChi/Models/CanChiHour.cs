namespace ZenTam.Api.Common.CanChi.Models;

/// <summary>
/// Can Chi for Hour (Giờ).
/// gioBatDau: 23=Tý, 1=Sửu, 3=Dần... (23 means start at Tý hour)
/// </summary>
/// <param name="Can">Heavenly Stem (Thiên Can)</param>
/// <param name="Chi">Earthly Branch (Địa Chi)</param>
public record CanChiHour(string Can, string Chi);