namespace ZenTam.Api.Common.Lunar.Models;

public sealed class TetResult
{
    public required int SolarDay { get; init; }
    public required int SolarMonth { get; init; }
    public required int SolarYear { get; init; }
    public required int LunarDay { get; init; }
    public required int LunarMonth { get; init; }
    public required int LunarYear { get; init; }
}