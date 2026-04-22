namespace ZenTam.Api.Features.ParseAndEvaluate.IntentParsing;

public record ParsedIntent(string? ActionCode, int? TargetYear, string Source);
// Source: "REGEX" | "SLM"
