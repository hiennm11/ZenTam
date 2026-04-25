namespace ZenTam.Api.Domain.Rules.MonthlyRuleEngine;

using Models;
using ZenTam.Api.Common.CanChi.Models;

public interface IMonthlyRuleEngine
{
    MonthlyEvaluationResult Evaluate(
        DateTime solarDate,
        int lunarDay,
        int lunarMonth,
        bool isLeap,
        CanChiDay canChiNgay,
        CanChiYear canChiTuoi);
}