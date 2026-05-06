namespace ZenTam.Api.Common.Lunar.Models;

public sealed class LunarDateResult
{
    public required int LunarYear { get; init; }
    public required int LunarMonth { get; init; }
    public required int LunarDay { get; init; }
    public required bool IsLeapMonth { get; init; }
    public required string GioHoangDao { get; init; }
}