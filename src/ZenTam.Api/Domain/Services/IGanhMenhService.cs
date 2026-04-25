namespace ZenTam.Api.Domain.Services;

using ZenTam.Api.Domain.Rules.MonthlyRuleEngine.Models;
using ZenTam.Api.Common.CanChi.Models;

public interface IGanhMenhService
{
    GanhMenhResult Evaluate(
        DateTime solarDate,
        int lunarDay,
        int lunarMonth,
        bool isLeap,
        CanChiDay canChiNgay,
        IEnumerable<FamilyMember> family);
}

public class FamilyMember
{
    public required string Name { get; init; }
    public required RelationshipType Relationship { get; init; }
    public required int BirthYear { get; init; }
    public required CanChiYear CanChiTuoi { get; init; }
}

public enum RelationshipType
{
    Vo,          // Wife
    Chong,       // Husband
    ConTrai,     // Son
    ConGai,      // Daughter
    Bo,          // Father
    Me,          // Mother
    Anh,         // Older Brother
    Chi,         // Older Sister
    Em           // Younger sibling
}

public class GanhMenhResult
{
    public required bool CanGanh { get; init; }
    public required int HighestSeverityAmongFamily { get; init; }
    public required IReadOnlyList<MemberEvaluation> MemberEvaluations { get; init; }
}

public class MemberEvaluation
{
    public required string Name { get; init; }
    public required RelationshipType Relationship { get; init; }
    public required DayVerdict Verdict { get; init; }
    public required int Severity { get; init; }
}