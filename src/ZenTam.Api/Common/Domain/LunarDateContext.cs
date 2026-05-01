namespace ZenTam.Api.Common.Domain;

public class LunarDateContext
{
    public int  LunarYear  { get; init; }
    public int  LunarMonth { get; init; }
    public int  LunarDay   { get; init; }
    public bool IsLeap     { get; init; }
    public int  Jdn       { get; init; }
}
