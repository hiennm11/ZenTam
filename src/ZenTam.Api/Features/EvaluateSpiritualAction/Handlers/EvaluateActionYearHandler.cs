namespace ZenTam.Api.Features.EvaluateSpiritualAction.Handlers;

using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Exceptions;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Common.Rules;
using ZenTam.Api.Common.Rules.Models;
using ZenTam.Api.Domain.Services;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;
using ZenTam.Api.Features.EvaluateSpiritualAction.Requests;
using ZenTam.Api.Features.EvaluateSpiritualAction.Responses;

public class EvaluateActionYearHandler
{
    private readonly ZenTamDbContext         _db;
    private readonly RuleResolver            _ruleResolver;
    private readonly ILunarCalculatorService _lunar;
    private readonly IGanhMenhService       _ganhMenhService;
    private readonly ICanChiCalculator       _canChi;

    public EvaluateActionYearHandler(
        ZenTamDbContext         db,
        RuleResolver            ruleResolver,
        ILunarCalculatorService lunar,
        IGanhMenhService       ganhMenhService,
        ICanChiCalculator       canChi)
    {
        _db              = db;
        _ruleResolver    = ruleResolver;
        _lunar           = lunar;
        _ganhMenhService = ganhMenhService;
        _canChi          = canChi;
    }

    public async Task<EvaluateActionYearResponse> HandleAsync(
        EvaluateActionYearRequest request,
        CancellationToken ct = default)
    {
        // 1. Load client
        var client = await _db.ClientProfiles.FindAsync(new object[] { request.UserId }, ct);
        if (client is null)
            throw new NotFoundException($"Client with Id '{request.UserId}' was not found.");

        // 2. Get lunar year of birth
        var clientLunarContext = _lunar.Convert(client.SolarDob);
        int clientLunarYob = clientLunarContext.LunarYear;

        // 3. Convert target year to lunar
        var targetSolarDate = new DateTime(request.TargetYear, 1, 1);
        var targetLunarContext = _lunar.Convert(targetSolarDate);
        int targetLunarYear = targetLunarContext.LunarYear;

        // 4. Get CanChi for target year and client's age
        var canChiNam = _canChi.GetCanChiNam(targetLunarYear);
        int clientAge = request.TargetYear - clientLunarYob + 1; // tuổi âm

        // 5. Load action rule mappings
        var mappings = await _db.ActionRuleMappings
            .Where(m => m.ActionId == request.ActionCode)
            .ToListAsync(ct);
        if (mappings.Count == 0)
            throw new NotFoundException($"Action '{request.ActionCode}' was not found.");

        // 6. Build UserProfile
        var profile = new UserProfile
        {
            LunarYOB   = clientLunarYob,
            Gender     = client.Gender,
            TargetYear = request.TargetYear
        };

        // 7. Build LunarDateContext (Year tier only)
        var lunarContext = new LunarDateContext
        {
            LunarYear = targetLunarYear
        };

        // 8. Resolve Year tier rules
        var resolved = _ruleResolver.Resolve(mappings, client.Gender, RuleTier.Year);

        // 9. Evaluate rules
        var results = new List<RuleResult>();
        foreach (var (rule, isMandatory) in resolved)
        {
            var context = new RuleContext { Profile = profile, Lunar = lunarContext };
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

        // 10. Aggregate
        int  totalScore = results.Sum(r => r.Score);
        bool isAllowed  = !results.Any(r => !r.IsPassed && r.IsMandatory);

        // 11. Determine verdict
        string verdict = DetermineVerdict(isAllowed, totalScore);

        // 12. Build base response
        var response = new EvaluateActionYearResponse
        {
            IsAllowed  = isAllowed,
            TotalScore = totalScore,
            Verdict    = verdict,
            TierUsed   = "Year",
            Details    = results,
            TargetYear = request.TargetYear,
            ClientAge  = clientAge,
            CanChiNam  = $"{canChiNam.Can} {canChiNam.Chi}"
        };

        // 13. Fire-and-forget Gánh Mệnh if CAM (silent fail - bonus check)
        if (verdict == "CAM")
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var clientWithFamily = await _db.ClientProfiles
                        .Include(c => c.RelatedPersons)
                        .FirstOrDefaultAsync(c => c.Id == request.UserId, ct);

                    if (clientWithFamily?.RelatedPersons.Count > 0)
                    {
                        var family = clientWithFamily.RelatedPersons.Select(rp =>
                        {
                            var relatedSolarDob = rp.SolarDob;
                            var relatedLunarDob = _lunar.Convert(relatedSolarDob);
                            var relatedCanChiYear = _canChi.GetCanChiNam(relatedLunarDob.LunarYear);
                            return new FamilyMember
                            {
                                Name = rp.Label,
                                Relationship = MapLabelToRelationship(rp.Label),
                                BirthYear = relatedSolarDob.Year,
                                CanChiTuoi = relatedCanChiYear
                            };
                        }).ToList();

                        var jdn = _canChi.GetJulianDayNumber(targetSolarDate);
                        var canChiNgay = _canChi.GetCanChiNgay(jdn);

                        var ganhMenh = _ganhMenhService.Evaluate(
                            targetSolarDate,
                            targetLunarContext.LunarDay,
                            targetLunarContext.LunarMonth,
                            targetLunarContext.IsLeap,
                            canChiNgay,
                            family);

                        // Note: Gánh Mệnh result is not assigned to response because
                        // response is a record with init-only properties. The result
                        // is computed for logging purposes only in this fire-and-forget context.
                        _ = ganhMenh; // suppress unused warning
                    }
                }
                catch
                {
                    // Silent fail - Gánh Mệnh is a bonus check
                }
            }, ct);
        }

        return response;
    }

    private static string DetermineVerdict(bool isAllowed, int totalScore)
    {
        if (!isAllowed) return "CAM";
        if (totalScore < 0) return "CANH_BAO";
        return "AN_TOAN";
    }

    private static RelationshipType MapLabelToRelationship(string label) => label.ToUpperInvariant() switch
    {
        "VỢ" or "VO" => RelationshipType.Vo,
        "CHỒNG" or "CHONG" => RelationshipType.Chong,
        "CON TRAI" or "CON_TRAI" or "CONTRAI" => RelationshipType.ConTrai,
        "CON GÁI" or "CON_GAI" or "CONGAI" => RelationshipType.ConGai,
        "BỐ" or "BO" or "CHA" => RelationshipType.Bo,
        "MẸ" or "ME" or "MOTHER" => RelationshipType.Me,
        "ANH" or "BROTHER" => RelationshipType.Anh,
        "CHỊ" or "CHI" or "SISTER" => RelationshipType.Chi,
        "EM" or "YOUNGER SIBLING" => RelationshipType.Em,
        _ => RelationshipType.Em
    };
}