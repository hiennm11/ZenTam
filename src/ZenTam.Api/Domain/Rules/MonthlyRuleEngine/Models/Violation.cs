namespace ZenTam.Api.Domain.Rules.MonthlyRuleEngine.Models;

public class Violation
{
    public required string RuleCode { get; init; }
    public required int Severity { get; init; }
    public required string Message { get; init; }
    public required bool IsBlocked { get; init; }
}