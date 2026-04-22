using System.Net.Http.Json;
using System.Text.Json;

namespace ZenTam.Api.Features.ParseAndEvaluate.IntentParsing;

public class SLMIntentParser(IHttpClientFactory httpClientFactory, ILogger<SLMIntentParser> logger) : IIntentParser
{
    private const string SystemPromptTemplate =
        "You are a strict JSON extraction bot. Current date context: {0} (Vietnam, UTC+7).\n\n" +
        "Extract from the user's message (which may be in Vietnamese slang):\n" +
        "- \"actionCode\": one of [XAY_NHA, CUOI_HOI, XUAT_HANH, MUA_XE]. Return null if unclear.\n" +
        "- \"targetYear\": integer year. Resolve relative terms: \"sang nam\"={1}, \"nam nay\"={0}, \"nam toi\"={1}.\n\n" +
        "RULES:\n" +
        "- Return ONLY valid JSON. No explanation. No markdown.\n" +
        "- If you cannot extract actionCode, return: {{\"actionCode\": null, \"targetYear\": null}}\n\n" +
        "Output format: {{\"actionCode\": \"XAY_NHA\", \"targetYear\": 2027}}";

    public async Task<ParsedIntent?> TryParseAsync(string text, int currentYear, CancellationToken ct = default)
    {
        try
        {
            string systemPrompt = string.Format(SystemPromptTemplate, currentYear, currentYear + 1);

            var requestBody = new
            {
                model = "gemini-3-flash", // or "qwen2-0.5b" based on deployment
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user",   content = text }
                }
            };

            var client = httpClientFactory.CreateClient("LiteLLM");
            using var response = await client.PostAsJsonAsync("/v1/chat/completions", requestBody, ct);
            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

            string? content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
                return null;

            using var parsed = JsonDocument.Parse(content);
            string? actionCode = parsed.RootElement.GetProperty("actionCode").GetString();

            if (actionCode is null)
                return null;

            int? targetYear = null;
            if (parsed.RootElement.TryGetProperty("targetYear", out var yearEl)
                && yearEl.ValueKind == JsonValueKind.Number)
            {
                targetYear = yearEl.GetInt32();
            }

            return new ParsedIntent(actionCode, targetYear ?? currentYear, "SLM");
        }
        catch (Exception ex) when (ex is JsonException
                                       or HttpRequestException
                                       or TaskCanceledException
                                       or OperationCanceledException)
        {
            logger.LogError(ex, "SLM parse failed for text: {Text}", text);
            return null;
        }
    }
}
