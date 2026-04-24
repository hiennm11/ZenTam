using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Exceptions;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Common.Rules;
using ZenTam.Api.Infrastructure;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Queries;

public class EvaluateActionHandler
{
    private readonly ZenTamDbContext         _db;
    private readonly RuleResolver            _ruleResolver;
    private readonly ILunarCalculatorService _lunar;

    public EvaluateActionHandler(
        ZenTamDbContext         db,
        RuleResolver            ruleResolver,
        ILunarCalculatorService lunar)
    {
        _db           = db;
        _ruleResolver = ruleResolver;
        _lunar        = lunar;
    }

    public async Task<EvaluateActionResponse> HandleAsync(
        EvaluateActionRequest request,
        CancellationToken ct = default)
    {
        // Step a: Load user
        var user = await _db.Users.FindAsync(new object[] { request.UserId }, ct);
        if (user is null)
            throw new NotFoundException($"User with Id '{request.UserId}' was not found.");

        // Step b: Load mappings
        var mappings = await _db.ActionRuleMappings
            .Where(m => m.ActionId == request.ActionCode)
            .ToListAsync(ct);
        if (mappings.Count == 0)
            throw new NotFoundException($"Action '{request.ActionCode}' was not found.");

        // Step c: Resolve rules (filtered by gender constraint)
        var resolved = _ruleResolver.Resolve(mappings, user.Gender);

        // Step d: Build UserProfile
        var profile = new UserProfile
        {
            LunarYOB   = user.LunarYOB,
            Gender     = user.Gender,
            TargetYear = request.TargetYear
        };

        // Step e: Get lunar context
        var lunarContext = _lunar.Convert(user.SolarDOB);

        // Step f: Evaluate each rule and attach IsMandatory from mapping
        var results = new List<RuleResult>();
        foreach (var (rule, isMandatory) in resolved)
        {
            var result = rule.Evaluate(profile, lunarContext);
            results.Add(new RuleResult
            {
                RuleName    = result.RuleName,
                IsPassed    = result.IsPassed,
                IsMandatory = isMandatory,
                Score       = result.Score,
                Message     = result.Message
            });
        }

        // Step g: Aggregate
        int  totalScore = results.Sum(r => r.Score);
        bool isAllowed  = !results.Any(r => !r.IsPassed && r.IsMandatory);

        // Step h: Verdict
        string verdict = isAllowed && totalScore == 0 ? "AN_TOAN"
                       : isAllowed && totalScore <  0 ? "CANH_BAO"
                       : "CAM";

        // Step i: Return
        return new EvaluateActionResponse
        {
            IsAllowed  = isAllowed,
            TotalScore = totalScore,
            Verdict    = verdict,
            Details    = results
        };
    }
}

