using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.CanChi.Models;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Exceptions;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Common.Rules;
using ZenTam.Api.Domain.Services;
using ZenTam.Api.Infrastructure;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Queries;

public class EvaluateActionHandler
{
    private readonly ZenTamDbContext          _db;
    private readonly RuleResolver             _ruleResolver;
    private readonly ILunarCalculatorService  _lunar;
    private readonly IGanhMenhService         _ganhMenhService;
    private readonly ICanChiCalculator        _canChi;

    public EvaluateActionHandler(
        ZenTamDbContext          db,
        RuleResolver             ruleResolver,
        ILunarCalculatorService  lunar,
        IGanhMenhService         ganhMenhService,
        ICanChiCalculator        canChi)
    {
        _db              = db;
        _ruleResolver    = ruleResolver;
        _lunar           = lunar;
        _ganhMenhService = ganhMenhService;
        _canChi          = canChi;
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

        // Step c: Resolve rules (filtered by gender constraint and Year tier for yearly evaluation)
        var resolved = _ruleResolver.Resolve(mappings, user.Gender, RuleTier.Year);

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

        // Step i: Build response (verdict starts as CAM)
        var response = new EvaluateActionResponse
        {
            IsAllowed  = isAllowed,
            TotalScore = totalScore,
            Verdict    = verdict,
            Details    = results
        };

        // Step j: Fire-and-forget Gánh Mệnh check if verdict is "CAM"
        if (verdict == "CAM")
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    // Find ClientProfile by UserId
                    var client = await _db.ClientProfiles
                        .Include(c => c.RelatedPersons)
                        .FirstOrDefaultAsync(c => c.Id == request.UserId, ct);

                    if (client is not null && client.RelatedPersons.Count > 0)
                    {
                        // Get lunar context for target year
                        var targetDate = new DateTime(request.TargetYear, 1, 1);
                        var targetLunarContext = _lunar.Convert(targetDate);

                        // Map related persons to FamilyMember objects
                        var family = client.RelatedPersons.Select(rp =>
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

                        // Get Can Chi for the day
                        var jdn = _canChi.GetJulianDayNumber(request.TargetYear < DateTime.Now.Year
                            ? new DateTime(request.TargetYear, 1, 1)
                            : DateTime.Now);
                        var canChiNgay = _canChi.GetCanChiNgay(jdn);

                        // Evaluate Gánh Mệnh
                        var ganhMenh = _ganhMenhService.Evaluate(
                            request.TargetYear < DateTime.Now.Year
                                ? new DateTime(request.TargetYear, 1, 1)
                                : DateTime.Now,
                            targetLunarContext.LunarDay,
                            targetLunarContext.LunarMonth,
                            targetLunarContext.IsLeap,
                            canChiNgay,
                            family);

                        // Always set GanhMenh with evaluation results (includes failed attempts)
                        response.GanhMenh = ganhMenh;

                        // Only upgrade verdict if someone can gánh
                        if (ganhMenh.CanGanh)
                        {
                            response.Verdict = "CANH_BAO";
                        }
                    }
                }
                catch
                {
                    // Silent fail - do not affect response
                }
            }, ct);
        }

        // Step k: Return response (Gánh Mệnh runs async in background)
        return response;
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
        "EM" or "Younger sibling" => RelationshipType.Em,
        _ => RelationshipType.Em // Default fallback
    };
}
