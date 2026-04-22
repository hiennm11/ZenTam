namespace ZenTam.Api.Features.ParseAndEvaluate.IntentParsing;

public interface IIntentParser
{
    Task<List<ParsedIntent>?> TryParseAsync(string text, int currentYear, CancellationToken ct = default);
}
