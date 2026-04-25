namespace ZenTam.Api.Domain.Services;

using ZenTam.Api.Domain.Rules.MonthlyRuleEngine;
using ZenTam.Api.Domain.Rules.MonthlyRuleEngine.Models;
using ZenTam.Api.Common.CanChi.Models;

public class GanhMenhService : IGanhMenhService
{
    private readonly IMonthlyRuleEngine _engine;

    public GanhMenhService(IMonthlyRuleEngine engine)
    {
        _engine = engine;
    }

    public GanhMenhResult Evaluate(
        DateTime solarDate,
        int lunarDay,
        int lunarMonth,
        bool isLeap,
        CanChiDay canChiNgay,
        IEnumerable<FamilyMember> family)
    {
        var sortedFamily = family
            .OrderBy(f => GetPriority(f.Relationship))
            .ToList();

        var memberEvaluations = new List<MemberEvaluation>();
        var highestSeverity = 0;

        foreach (var member in sortedFamily)
        {
            var result = _engine.Evaluate(
                solarDate, lunarDay, lunarMonth, isLeap, canChiNgay, member.CanChiTuoi);

            memberEvaluations.Add(new MemberEvaluation
            {
                Name = member.Name,
                Relationship = member.Relationship,
                Verdict = result.OverallVerdict,
                Severity = result.OverallSeverity
            });

            if (result.OverallSeverity > highestSeverity)
            {
                highestSeverity = result.OverallSeverity;
            }
        }

        // CanGanh = true if any member has safe verdict (severity <= 1, i.e., Bình or Cát)
        bool canGanh = memberEvaluations.Any(m => m.Severity <= 1);

        return new GanhMenhResult
        {
            CanGanh = canGanh,
            HighestSeverityAmongFamily = highestSeverity,
            MemberEvaluations = memberEvaluations
        };
    }

    private static int GetPriority(RelationshipType relationship) => relationship switch
    {
        RelationshipType.Vo => 0,
        RelationshipType.Chong => 1,
        RelationshipType.ConTrai => 2,
        RelationshipType.ConGai => 3,
        RelationshipType.Bo => 4,
        RelationshipType.Me => 5,
        RelationshipType.Anh => 6,
        RelationshipType.Chi => 7,
        RelationshipType.Em => 8,
        _ => 9
    };
}