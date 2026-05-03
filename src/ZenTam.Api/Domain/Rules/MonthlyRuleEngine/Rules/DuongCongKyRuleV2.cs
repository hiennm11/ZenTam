namespace ZenTam.Api.Domain.Rules.MonthlyRuleEngine.Rules;

using Models;
using ZenTam.Api.Common.Rules;
using ZenTam.Api.Common.Rules.Models;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Common.CanChi;

/// <summary>
/// Dương Công Kỵ Nhật V2 (ISpiritualRule) - 13 ngày xấu nhất trong năm theo Thập Bát Tú.
/// JDN đầu năm âm lịch + 12, cứ cách 12 ngày.
/// Offset range [12, 168], every 12 days.
/// </summary>
public sealed class DuongCongKyRuleV2 : ISpiritualRule
{
    private readonly ILunarCalculatorService _lunarCalculator;
    private readonly ICanChiCalculator _canChiCalculator;

    public DuongCongKyRuleV2(ILunarCalculatorService lunarCalculator, ICanChiCalculator canChiCalculator)
    {
        _lunarCalculator = lunarCalculator;
        _canChiCalculator = canChiCalculator;
    }

    public string RuleCode => "DUONG_CONG_KY";

    public RuleEvaluation Evaluate(RuleContext context)
    {
        var jdn = context.Lunar.Jdn;

        // Get lunar year from the context
        var lunarYear = context.Lunar.LunarYear;

        // If lunar year is 0, we can't compute - return pass
        if (lunarYear == 0)
        {
            return new RuleEvaluation
            {
                RuleCode = RuleCode,
                IsPassed = true,
                ScoreImpact = 0,
                Severity = RuleSeverity.None,
                IsBlocked = false,
                IsMandatory = false,
                Message = "Không có đủ thông tin để tính Dương Công Kỵ"
            };
        }

        // Get JDN of first day of lunar year (mùng 1 tháng giêng)
        var lunarYearStartJdn = _lunarCalculator.GetLunarNewYearJdn(lunarYear);

        var offset = jdn - lunarYearStartJdn;

        // Must be between first day (offset >= 12) and within 13 bad days (offset <= 168)
        if (offset < 12 || offset > 168)
        {
            return new RuleEvaluation
            {
                RuleCode = RuleCode,
                IsPassed = true,
                ScoreImpact = 0,
                Severity = RuleSeverity.None,
                IsBlocked = false,
                IsMandatory = false,
                Message = $"Không phạm Dương Công Kỵ (ngày {context.Lunar.LunarDay} tháng {context.Lunar.LunarMonth})"
            };
        }

        // Check if offset matches the pattern: 12, 24, 36, ... 168
        // Formula: (offset - 12) % 12 == 0
        if ((offset - 12) % 12 != 0)
        {
            return new RuleEvaluation
            {
                RuleCode = RuleCode,
                IsPassed = true,
                ScoreImpact = 0,
                Severity = RuleSeverity.None,
                IsBlocked = false,
                IsMandatory = false,
                Message = $"Không phạm Dương Công Kỵ (ngày {context.Lunar.LunarDay} tháng {context.Lunar.LunarMonth})"
            };
        }

        return new RuleEvaluation
        {
            RuleCode = RuleCode,
            IsPassed = false,
            ScoreImpact = -15,
            Severity = RuleSeverity.DaiHung,
            IsBlocked = true,
            IsMandatory = true,
            Message = $"Dương Công Kỵ Nhật — ngày đại kỵ mọi việc lớn (offset: {offset})"
        };
    }
}
