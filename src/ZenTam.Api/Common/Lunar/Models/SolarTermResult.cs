namespace ZenTam.Api.Common.Lunar.Models;

public sealed class SolarTermResult
{
    public required string Name { get; init; }
    public required string ChineseName { get; init; }
    public required int SolarDay { get; init; }
    public required int SolarMonth { get; init; }
    public required int SolarYear { get; init; }
    public required string GioBatDau { get; init; }
}