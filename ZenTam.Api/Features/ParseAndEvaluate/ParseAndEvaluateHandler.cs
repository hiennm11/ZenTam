using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using ZenTam.Api.Common.Caching;
using ZenTam.Api.Common.Exceptions;
using ZenTam.Api.Features.EvaluateSpiritualAction;
using ZenTam.Api.Features.ParseAndEvaluate.IntentParsing;

namespace ZenTam.Api.Features.ParseAndEvaluate;

public class ParseAndEvaluateHandler
{
    private readonly RegexIntentParser      _regexParser;
    private readonly SLMIntentParser        _slmParser;
    private readonly ICacheService          _cache;
    private readonly EvaluateActionHandler  _evaluateHandler;
    private readonly ILogger<ParseAndEvaluateHandler> _logger;

    public ParseAndEvaluateHandler(
        RegexIntentParser     regexParser,
        SLMIntentParser       slmParser,
        ICacheService         cache,
        EvaluateActionHandler evaluateHandler,
        ILogger<ParseAndEvaluateHandler> logger)
    {
        _regexParser     = regexParser;
        _slmParser       = slmParser;
        _cache           = cache;
        _evaluateHandler = evaluateHandler;
        _logger          = logger;
    }

    public async Task<List<EvaluateActionResponse>> HandleAsync(
        ParseAndEvaluateRequest request,
        CancellationToken ct = default)
    {
        // Step a — Get current year (Vietnam UTC+7)
        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        int currentYear = TimeZoneInfo.ConvertTime(DateTime.UtcNow, vnTimeZone).Year;

        // // Step b — Try Regex
        // var intents = await _regexParser.TryParseAsync(request.Text, currentYear, ct);

        // // Step c/d — Fall back to SLM if Regex missed
        // if (intents is null)
        // {
        //     intents = await _slmParser.TryParseAsync(request.Text, currentYear, ct);
        //     if (intents is null)
        //         throw new UnprocessableEntityException("Không thể hiểu yêu cầu, vui lòng nói rõ hơn.");
        // }

        var intents = await _slmParser.TryParseAsync(request.Text, currentYear, ct);
            if (intents is null)
                throw new UnprocessableEntityException("Không thể hiểu yêu cầu, vui lòng nói rõ hơn.");

        // Step e/f/g — Evaluate each intent (cache-aside per intent)
        var results = new List<EvaluateActionResponse>(intents.Count);
        foreach (var intent in intents)
        {
            var cacheKey = BuildCacheKey(request.UserId, intent.ActionCode!, intent.TargetYear!.Value);

            var cached = await _cache.GetAsync<EvaluateActionResponse>(cacheKey, ct);
            if (cached is not null)
            {
                _logger.LogInformation("Cache HIT ({Source} path) for key {Key}", intent.Source, cacheKey);
                results.Add(cached);
                continue;
            }

            var evalRequest = new EvaluateActionRequest
            {
                UserId     = request.UserId,
                ActionCode = intent.ActionCode!,
                TargetYear = intent.TargetYear!.Value
            };
            var result = await _evaluateHandler.HandleAsync(evalRequest, ct);

            await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(24), ct);
            results.Add(result);
        }

        return results;
    }

    private static string BuildCacheKey(Guid userId, string actionCode, int targetYear)
    {
        var raw  = $"{userId}:{actionCode}:{targetYear}";
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(raw));
        return $"zentam:eval:{Convert.ToHexString(hash).ToLower()}";
    }
}
