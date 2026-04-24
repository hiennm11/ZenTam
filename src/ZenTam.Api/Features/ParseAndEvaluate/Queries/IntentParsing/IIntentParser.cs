namespace ZenTam.Api.Features.ParseAndEvaluate.Queries.IntentParsing;

public interface IIntentParser
{
    Task<List<ParsedIntent>?> TryParseAsync(string text, int currentYear, CancellationToken ct = default);
}
