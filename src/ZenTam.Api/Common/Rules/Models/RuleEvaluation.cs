namespace ZenTam.Api.Common.Rules.Models;

public class RuleEvaluation
{
    public required string RuleCode { get; init; }
    public required bool IsPassed { get; init; }
    public required int ScoreImpact { get; init; }
    public required RuleSeverity Severity { get; init; }
    public required bool IsBlocked { get; init; }
    public required bool IsMandatory { get; init; }
    public required string Message { get; init; }
}