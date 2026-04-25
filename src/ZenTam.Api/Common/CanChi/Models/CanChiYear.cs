namespace ZenTam.Api.Common.CanChi.Models;

/// <summary>
/// Can Chi for Year (Năm).
/// Can: Giáp, Ất, Bính, Đinh, Mậu, Kỷ, Canh, Tân, Nhâm, Quý
/// Chi: Tý, Sửu, Dần, Mão, Thìn, Tỵ, Ngọ, Mùi, Thân, Dậu, Tuất, Hợi
/// </summary>
/// <param name="Can">Heavenly Stem (Thiên Can)</param>
/// <param name="Chi">Earthly Branch (Địa Chi)</param>
public record CanChiYear(string Can, string Chi);