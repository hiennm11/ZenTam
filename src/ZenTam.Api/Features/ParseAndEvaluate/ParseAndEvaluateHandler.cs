using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZenTam.Api.Common.Caching;
using ZenTam.Api.Common.Exceptions;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Features.EvaluateSpiritualAction;
using ZenTam.Api.Features.ParseAndEvaluate.IntentParsing;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.Features.ParseAndEvaluate;

public class ParseAndEvaluateHandler
{
    private readonly RegexIntentParser       _regexParser;
    private readonly SLMIntentParser         _slmParser;
    private readonly ICacheService           _cache;
    private readonly EvaluateActionHandler   _evaluateHandler;
    private readonly ILunarCalculatorService  _lunarCalculator;
    private readonly ZenTamDbContext         _dbContext;
    private readonly ILogger<ParseAndEvaluateHandler> _logger;

    public ParseAndEvaluateHandler(
        RegexIntentParser      regexParser,
        SLMIntentParser        slmParser,
        ICacheService          cache,
        EvaluateActionHandler  evaluateHandler,
        ILunarCalculatorService lunarCalculator,
        ZenTamDbContext        dbContext,
        ILogger<ParseAndEvaluateHandler> logger)
    {
        _regexParser     = regexParser;
        _slmParser       = slmParser;
        _cache           = cache;
        _evaluateHandler = evaluateHandler;
        _lunarCalculator = lunarCalculator;
        _dbContext       = dbContext;
        _logger          = logger;
    }

    public async Task<List<EvaluateActionResponse>> HandleAsync(
        ParseAndEvaluateRequest request,
        CancellationToken ct = default)
    {
        // Step 1 — Validate ClientId
        if (!request.ClientId.HasValue)
            throw new ValidationException("ClientId is required");

        // Step 2 — Load ClientProfile from database
        var client = await _dbContext.ClientProfiles
            .FirstOrDefaultAsync(c => c.Id == request.ClientId.Value, ct);

        if (client is null)
            throw new NotFoundException($"Client {request.ClientId} not found");

        // Step 3 — Compute lunar context from ClientProfile.SolarDob
        var lunarCtx = _lunarCalculator.Convert(client.SolarDob);
        int lunarYear = lunarCtx.LunarYear;

        // Step 4 — Get current year (Vietnam UTC+7)
        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        int currentYear = TimeZoneInfo.ConvertTime(DateTime.UtcNow, vnTimeZone).Year;

        // Step 5 — Try Regex first, then fall back to SLM
        var intents = await _regexParser.TryParseAsync(request.Message, currentYear, ct);

        if (intents is null)
        {
            // Check Redis LLM Intent Cache first (fail-open on Redis failure)
            var cacheKey = ComputeIntentCacheKey(request.Message, lunarYear);
            var cachedIntent = await TryGetCachedIntentAsync(cacheKey, ct);

            if (cachedIntent is not null)
            {
                _logger.LogInformation("Redis LLM Intent Cache HIT for key {Key}", cacheKey);
                intents = new List<ParsedIntent> { cachedIntent };
            }
            else
            {
                // Cache miss — call SLM LLM
                intents = await _slmParser.TryParseAsync(request.Message, currentYear, ct);
                if (intents is null)
                    throw new UnprocessableEntityException("Không thể hiểu yêu cầu, vui lòng nói rõ hơn.");

                // Cache the result with 24h TTL
                foreach (var intent in intents)
                {
                    if (intent.ActionCode is not null)
                    {
                        var intentCacheKey = ComputeIntentCacheKey(intent.ActionCode, lunarYear);
                        await TrySetCachedIntentAsync(intentCacheKey, intent, TimeSpan.FromHours(24), ct);
                    }
                }
            }
        }

        // Step 6 — Evaluate each intent
        var results = new List<EvaluateActionResponse>(intents.Count);
        foreach (var intent in intents)
        {
            var evalCacheKey = BuildEvalCacheKey(intent.ActionCode!, intent.TargetYear!.Value);

            var cached = await _cache.GetAsync<EvaluateActionResponse>(evalCacheKey, ct);
            if (cached is not null)
            {
                _logger.LogInformation("Eval Cache HIT for key {Key}", evalCacheKey);
                results.Add(cached);
                continue;
            }

            var evalRequest = new EvaluateActionRequest
            {
                UserId     = request.ClientId.Value,  // Using ClientId as UserId for rule evaluation
                ActionCode = intent.ActionCode!,
                TargetYear = intent.TargetYear!.Value
            };
            var result = await _evaluateHandler.HandleAsync(evalRequest, ct);

            await _cache.SetAsync(evalCacheKey, result, TimeSpan.FromHours(24), ct);
            results.Add(result);
        }

        // Step 7 — Write ConsultationSession
        var session = new ConsultationSession
        {
            Id = Guid.NewGuid(),
            ClientId = request.ClientId.Value,
            RawMessage = request.Message,
            ParsedIntents = JsonSerializer.Serialize(intents),
            EvaluationResult = JsonSerializer.Serialize(results),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.ConsultationSessions.Add(session);
        await _dbContext.SaveChangesAsync(ct);

        return results;
    }

    private async Task<ParsedIntent?> TryGetCachedIntentAsync(string cacheKey, CancellationToken ct)
    {
        try
        {
            return await _cache.GetAsync<ParsedIntent>(cacheKey, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis LLM Intent Cache GET failed for key {Key}, failing open", cacheKey);
            return null;
        }
    }

    private async Task TrySetCachedIntentAsync(string cacheKey, ParsedIntent intent, TimeSpan ttl, CancellationToken ct)
    {
        try
        {
            await _cache.SetAsync(cacheKey, intent, ttl, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis LLM Intent Cache SET failed for key {Key}, failing open", cacheKey);
        }
    }

    /// <summary>
    /// Compute SHA256 hash for intent cache key.
    /// Format: SHA256("{ActionCode}|{LunarYear}")
    /// </summary>
    private static string ComputeIntentCacheKey(string actionCode, int lunarYear)
    {
        var raw = $"{actionCode}|{lunarYear}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return $"zentam:intent:{Convert.ToHexString(hash).ToLower()}";
    }

    private static string BuildEvalCacheKey(string actionCode, int targetYear)
    {
        var raw  = $"{actionCode}:{targetYear}";
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(raw));
        return $"zentam:eval:{Convert.ToHexString(hash).ToLower()}";
    }
}
