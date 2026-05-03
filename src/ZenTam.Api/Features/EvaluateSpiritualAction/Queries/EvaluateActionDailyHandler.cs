using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Exceptions;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Common.Rules;
using ZenTam.Api.Common.Rules.Models;
using ZenTam.Api.Features.Calendars.Services;
using ZenTam.Api.Infrastructure;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Queries;

/// <summary>
/// Handler for evaluating spiritual actions on a specific day (Day-tier).
/// </summary>
public class EvaluateActionDailyHandler
{
    private readonly ZenTamDbContext _db;
    private readonly RuleResolver _ruleResolver;
    private readonly ILunarCalculatorService _lunar;
    private readonly ICanChiCalculator _canChi;
    private readonly ISolarTermCalculator _solarTerm;

    public EvaluateActionDailyHandler(
        ZenTamDbContext db,
        RuleResolver ruleResolver,
        ILunarCalculatorService lunar,
        ICanChiCalculator canChi,
        ISolarTermCalculator solarTerm)
    {
        _db = db;
        _ruleResolver = ruleResolver;
        _lunar = lunar;
        _canChi = canChi;
        _solarTerm = solarTerm;
    }

    public async Task<EvaluateActionResponse> HandleAsync(
        EvaluateActionDailyRequest request,
        CancellationToken ct = default)
    {
        // Step A: Load client
        var client = await _db.ClientProfiles.FindAsync(new object[] { request.UserId }, ct);
        if (client is null)
            throw new NotFoundException($"Client with Id '{request.UserId}' was not found.");

        // Step B: Load mappings
        var mappings = await _db.ActionRuleMappings
            .Where(m => m.ActionId == request.ActionCode)
            .ToListAsync(ct);
        if (mappings.Count == 0)
            throw new NotFoundException($"Action '{request.ActionCode}' was not found.");

        // Step C: Resolve Day-tier rules
        var resolved = _ruleResolver.Resolve(mappings, client.Gender, RuleTier.Day);

        // Step D: Build lunar context for target date
        var targetDateTime = request.TargetDate.ToDateTime(TimeOnly.MinValue);
        var lunarContext = _lunar.Convert(targetDateTime);
        var jdn = _canChi.GetJulianDayNumber(targetDateTime);

        // Update JDN and SolarMonth in lunar context (needed by rules)
        int solarMonth = _solarTerm.GetSolarMonth(targetDateTime);
        lunarContext = new LunarDateContext
        {
            LunarYear = lunarContext.LunarYear,
            LunarMonth = lunarContext.LunarMonth,
            LunarDay = lunarContext.LunarDay,
            IsLeap = lunarContext.IsLeap,
            Jdn = jdn,
            SolarMonth = solarMonth
        };

        // Step E: Build UserProfile
        var profile = new UserProfile
        {
            LunarYOB = lunarContext.LunarYear,
            Gender = client.Gender,
            TargetYear = request.TargetDate.Year
        };

        // Step F: Evaluate each rule
        var results = new List<RuleResult>();
        foreach (var (rule, isMandatory) in resolved)
        {
            var context = new RuleContext { Profile = profile, Lunar = lunarContext };
            var result = rule.Evaluate(context);
            results.Add(new RuleResult
            {
                RuleName = result.RuleCode,
                IsPassed = result.IsPassed,
                IsMandatory = isMandatory,
                Score = result.ScoreImpact,
                Message = result.Message
            });
        }

        // Step G: Aggregate
        int totalScore = results.Sum(r => r.Score);
        bool isAllowed = !results.Any(r => !r.IsPassed && r.IsMandatory);

        // Step H: Verdict
        string verdict = isAllowed && totalScore == 0 ? "AN_TOAN"
                       : isAllowed && totalScore < 0  ? "CANH_BAO"
                       : "CAM";

        // Step I: Return (NO Gánh Mệnh for Day tier)
        return new EvaluateActionResponse
        {
            IsAllowed = isAllowed,
            TotalScore = totalScore,
            Verdict = verdict,
            Details = results,
            GanhMenh = null // Day tier never has Gánh Mệnh
        };
    }
}