namespace ZenTam.Api.Common.CanChi.Models;

/// <summary>
/// Full Can Chi context for Monthly Rule Engine.
/// Combines year, month, day stem-branch with special cycles.
/// </summary>
public class CanChiInfo
{
    /// <summary>
    /// Can Chi for the lunar year.
    /// </summary>
    public required CanChiYear CanChiNam { get; init; }

    /// <summary>
    /// Can Chi for the lunar month.
    /// </summary>
    public required CanChiMonth CanChiThang { get; init; }

    /// <summary>
    /// Can Chi for the day.
    /// </summary>
    public required CanChiDay CanChiNgay { get; init; }

    /// <summary>
    /// Thập Nhị Trực (12 Earthly Branches for days).
    /// Index 0-11: Kiến, Trừ, Mãn, Bình, Định, Chấp, Phá, Nguy, Thành, Thu, Khai, Bế
    /// </summary>
    public int ThapNhiTruc { get; init; }

    /// <summary>
    /// Nhị Thập Bát Tú (28 Lunar Mansions).
    /// Index 0-27.
    /// </summary>
    public int NhiThapBatTu { get; init; }
}