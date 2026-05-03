namespace ZenTam.Api.Common.Rules;

/// <summary>
/// Result DTO returned by rule evaluation.
/// Used in API response payloads (EvaluateActionResponse.Details).
/// </summary>
public class RuleResult
{
    public required string RuleName    { get; init; }
    public required bool   IsPassed   { get; init; }
    public required bool   IsMandatory{ get; init; }
    public required int    Score      { get; init; }
    public required string Message     { get; init; }
}
