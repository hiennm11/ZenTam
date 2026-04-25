namespace ZenTam.Api.Domain.Rules.MonthlyRuleEngine.Models;

public class MonthlyEvaluationResult
{
    public required DayVerdict OverallVerdict { get; init; }
    public required int OverallSeverity { get; init; }
    public required DayLevel DayLevel { get; init; }
    public required IReadOnlyList<Violation> Violations { get; init; }
    public required DateTime EvaluatedAt { get; init; }
}