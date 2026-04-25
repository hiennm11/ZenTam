namespace ZenTam.Api.Domain.Rules.MonthlyRuleEngine.Rules;

using Models;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Common.CanChi;

/// <summary>
/// Dương Công Kỵ Nhật - 13 ngày xấu nhất trong năm theo Thập Bát Tú.
/// JDN đầu năm âm lịch + 12, cứ cách 12 ngày.
/// Severity: 4 (Đại Hung)
/// </summary>
public sealed class DuongCongKyRule : IMonthlyRule
{
    private readonly ILunarCalculatorService _lunarCalculator;
    private readonly ICanChiCalculator _canChiCalculator;

    public DuongCongKyRule(ILunarCalculatorService lunarCalculator, ICanChiCalculator canChiCalculator)
    {
        _lunarCalculator = lunarCalculator;
        _canChiCalculator = canChiCalculator;
    }

    public string RuleCode => "DUONG_CONG_KY";

    public Violation? Evaluate(int lunarDay, int lunarMonth, DateTime? solarDate, string chiNgay, Common.CanChi.Models.CanChiYear? canChiTuoi)
    {
        if (solarDate == null)
            return null;

        // Get JDN for the solar date
        var jdn = _canChiCalculator.GetJulianDayNumber(solarDate.Value);
        
        // Get lunar year from the date
        var lunarContext = _lunarCalculator.Convert(solarDate.Value);
        var lunarYear = lunarContext.LunarYear;
        
        // Get JDN of first day of lunar year (mùng 1 tháng giêng)
        var lunarYearStartJdn = _lunarCalculator.GetLunarNewYearJdn(lunarYear);
        
        // Check if current JDN falls on a Dương Công Kỵ day
        // Days are: lunarYearStartJdn + 12, +24, +36, ... +168 (13 days total)
        var offset = jdn - lunarYearStartJdn;
        
        // Must be between first day (offset >= 12) and within 13 bad days (offset <= 168)
        if (offset < 12 || offset > 168)
            return null;
            
        // Check if offset matches the pattern: 12, 24, 36, ... 168
        // Formula: (offset - 12) % 12 == 0
        if ((offset - 12) % 12 != 0)
            return null;

        return new Violation
        {
            RuleCode = RuleCode,
            Severity = 4,
            Message = "Ngày Dương Công Kỵ Nhật - đại kỵ mọi việc lớn",
            IsBlocked = true
        };
    }
}