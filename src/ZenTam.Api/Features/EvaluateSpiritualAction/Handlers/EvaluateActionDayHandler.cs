namespace ZenTam.Api.Features.EvaluateSpiritualAction.Handlers;

using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Exceptions;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Common.Rules;
using ZenTam.Api.Common.Rules.Models;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;
using ZenTam.Api.Features.EvaluateSpiritualAction.Requests;
using ZenTam.Api.Features.EvaluateSpiritualAction.Responses;

public class EvaluateActionDayHandler
{
    private readonly ZenTamDbContext         _db;
    private readonly RuleResolver            _ruleResolver;
    private readonly ILunarCalculatorService _lunar;
    private readonly ICanChiCalculator       _canChi;

    public EvaluateActionDayHandler(
        ZenTamDbContext         db,
        RuleResolver            ruleResolver,
        ILunarCalculatorService lunar,
        ICanChiCalculator       canChi)
    {
        _db              = db;
        _ruleResolver    = ruleResolver;
        _lunar           = lunar;
        _canChi          = canChi;
    }

    public async Task<EvaluateActionDayResponse> HandleAsync(
        EvaluateActionDayRequest request,
        CancellationToken ct = default)
    {
        // 1. Load client
        var client = await _db.ClientProfiles.FindAsync(new object[] { request.UserId }, ct);
        if (client is null)
            throw new NotFoundException($"Client with Id '{request.UserId}' was not found.");

        // 2. Get lunar year of birth
        var clientLunarContext = _lunar.Convert(client.SolarDob);
        int clientLunarYob = clientLunarContext.LunarYear;

        // 3. Get JDN for target date
        DateTime targetSolarDate = request.TargetDate.ToDateTime(TimeOnly.MinValue);
        int jdn = _canChi.GetJulianDayNumber(targetSolarDate);

        // 4. Convert target solar date to lunar
        var targetLunarContext = _lunar.Convert(targetSolarDate);

        // 5. Get CanChi for year, month, day, and client's age
        var canChiNam = _canChi.GetCanChiNam(targetLunarContext.LunarYear);
        var canChiThang = _canChi.GetCanChiThang(
            targetLunarContext.LunarYear,
            targetLunarContext.LunarMonth,
            targetLunarContext.IsLeap);
        var canChiNgay = _canChi.GetCanChiNgay(jdn);
        var canChiTuoi = _canChi.GetCanChiNam(clientLunarYob);

        // 6. Get TrucNgay
        int trucIndex = _canChi.GetThapNhiTruc(targetSolarDate);
        string trucNgay = _canChi.GetTrucName(trucIndex);

        // 7. Load action rule mappings
        var mappings = await _db.ActionRuleMappings
            .Where(m => m.ActionId == request.ActionCode)
            .ToListAsync(ct);
        if (mappings.Count == 0)
            throw new NotFoundException($"Action '{request.ActionCode}' was not found.");

        // 8. Build UserProfile
        var profile = new UserProfile
        {
            LunarYOB   = clientLunarYob,
            Gender     = client.Gender,
            TargetYear = targetSolarDate.Year
        };

        // 9. Build LunarDateContext with Year + Month + Day
        var lunarContext = new LunarDateContext
        {
            LunarYear  = targetLunarContext.LunarYear,
            LunarMonth = targetLunarContext.LunarMonth,
            LunarDay   = targetLunarContext.LunarDay,
            IsLeap     = targetLunarContext.IsLeap,
            Jdn        = jdn,
            SolarMonth = targetSolarDate.Month
        };

        // 10. Resolve Year + Month + Day tier rules
        var resolvedYear = _ruleResolver.Resolve(mappings, client.Gender, RuleTier.Year);
        var resolvedMonth = _ruleResolver.Resolve(mappings, client.Gender, RuleTier.Month);
        var resolvedDay = _ruleResolver.Resolve(mappings, client.Gender, RuleTier.Day);
        var resolved = resolvedYear.Concat(resolvedMonth).Concat(resolvedDay).ToList();

        // 11. Evaluate rules
        var results = new List<RuleResult>();
        foreach (var (rule, isMandatory) in resolved)
        {
            var context = new RuleContext
            {
                Profile = profile,
                Lunar = lunarContext,
                CanChiNgay = canChiNgay,
                CanChiTuoi = canChiTuoi
            };
            var result = rule.Evaluate(context);
            results.Add(new RuleResult
            {
                RuleName    = result.RuleCode,
                IsPassed    = result.IsPassed,
                IsMandatory = isMandatory,
                Score       = result.ScoreImpact,
                Message     = result.Message
            });
        }

        // 12. Aggregate
        int  totalScore = results.Sum(r => r.Score);
        bool isAllowed  = !results.Any(r => !r.IsPassed && r.IsMandatory);

        // 13. Determine verdict (no Gánh Mệnh for Day tier)
        string verdict = DetermineVerdict(isAllowed, totalScore);

        // 14. Build LunarDateStr like "14/3 Bính Ngọ"
        string lunarDateStr = $"{targetLunarContext.LunarDay}/{targetLunarContext.LunarMonth} {canChiNam.Can} {canChiNam.Chi}";

        // 15. Return response
        return new EvaluateActionDayResponse
        {
            IsAllowed   = isAllowed,
            TotalScore  = totalScore,
            Verdict     = verdict,
            TierUsed    = "Day",
            Details     = results,
            // No GanhMenh for Day tier (response inherits null from base)
            TargetDate  = request.TargetDate,
            LunarDateStr = lunarDateStr,
            CanChiNgay  = $"{canChiNgay.Can} {canChiNgay.Chi}",
            TrucNgay    = trucNgay
        };
    }

    private static string DetermineVerdict(bool isAllowed, int totalScore)
    {
        if (!isAllowed) return "CAM";
        if (totalScore < 0) return "CANH_BAO";
        return "AN_TOAN";
    }
}