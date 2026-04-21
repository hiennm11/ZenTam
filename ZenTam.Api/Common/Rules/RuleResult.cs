namespace ZenTam.Api.Common.Rules;

public class RuleResult
{
    public string RuleName    { get; init; } = string.Empty;
    public bool   IsPassed    { get; init; }
    public bool   IsMandatory { get; init; }
    public int    Score       { get; init; }
    public string Message     { get; init; } = string.Empty;
}
