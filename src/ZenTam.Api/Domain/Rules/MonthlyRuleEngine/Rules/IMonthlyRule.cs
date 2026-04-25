namespace ZenTam.Api.Domain.Rules.MonthlyRuleEngine.Rules;

using Models;
using ZenTam.Api.Common.CanChi.Models;

public interface IMonthlyRule
{
    string RuleCode { get; }
    Violation? Evaluate(int lunarDay, int lunarMonth, DateTime? solarDate, string chiNgay, CanChiYear? canChiTuoi);
}