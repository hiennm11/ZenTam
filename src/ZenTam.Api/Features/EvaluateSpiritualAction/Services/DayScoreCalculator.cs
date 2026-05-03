using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Common.Rules;
using ZenTam.Api.Common.Rules.Models;
using ZenTam.Api.Features.Calendars.Models;
using ZenTam.Api.Features.Calendars.Services;
using ZenTam.Api.Features.EvaluateSpiritualAction.Data;
using ZenTam.Api.Features.EvaluateSpiritualAction.Models;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Services;

public class DayScoreCalculator(
    IDayContextService dayContextService,
    ILunarCalculatorService lunarCalculator,
    ICanChiCalculator canChiCalculator,
    ActionCodeMapper actionCodeMapper,
    RuleResolver ruleResolver
) : IDayScoreCalculator
{
    private const int MaxScore = 80;

    public DayScoreResult Calculate(DateTime solarDate, ActionCode action, Gender userGender, RuleTier tier, int? clientLunarYear = null)
    {
        var dayContext = dayContextService.GetDayContext(solarDate);
        var lunar = lunarCalculator.Convert(solarDate);

        int score = 0;
        var reasons = new List<string>();

        // 1. Trực-Action matrix score
        int trucScore = TrucActionScoreTable.GetScore((TrucType)dayContext.TrucIndex, action);
        score += trucScore;
        reasons.Add(trucScore >= 0
            ? $"Trực {dayContext.TrucName} tốt cho {GetActionName(action)}"
            : $"Trực {dayContext.TrucName} xấu cho {GetActionName(action)}");

        // 2. Tú classification
        int tuScore = dayContext.NhiThapBatTu.Classification switch
        {
            TuClassification.Kiettu => 6,
            TuClassification.Binhtu => 0,
            TuClassification.Hungtu => -6,
            _ => 0
        };
        score += tuScore;
        reasons.Add(tuScore switch
        {
            > 0 => dayContext.NhiThapBatTu.Name + " (Kiết Tú)",
            < 0 => dayContext.NhiThapBatTu.Name + " (Hưng Tú)",
            _ => dayContext.NhiThapBatTu.Name + " (Bình Tú)"
        });

        // 3. Hoàng Đạo
        if (dayContext.HoangDao.IsHoangDao)
        {
            score += 6;
            reasons.Add("Hoàng Đạo");
        }

        // 4. Not Xung Tuổi (clientLunarYear vs CanChiNgay)
        bool isXungTuoi = false;
        if (clientLunarYear.HasValue)
        {
            var clientCanChi = canChiCalculator.GetCanChiNam(clientLunarYear.Value);
            var dayCanChi = canChiCalculator.GetCanChiNgay(lunarCalculator.GetJulianDayNumber(solarDate.Year, solarDate.Month, solarDate.Day));
            isXungTuoi = IsXungChi(clientCanChi.Chi, dayCanChi.Chi);

            if (!isXungTuoi)
            {
                score += 12;
                reasons.Add("Không xung tuổi");
            }
            else
            {
                reasons.Add("Xung tuổi");
            }
        }

        // 5. Not Ngày Kỵ
        if (!dayContext.IsNgayKy)
        {
            score += 12;
            reasons.Add("Không ngày Kỵ");
        }
        else
        {
            reasons.Add("Ngày Kỵ (5, 14, 23)");
        }

        // 6. Not Sát Chủ
        if (!dayContext.SatChu.IsSatChu)
        {
            score += 12;
            reasons.Add("Không Sát Chủ");
        }
        else
        {
            reasons.Add("Sát Chủ");
        }

        // 7. Not Thọ Tử
        if (!dayContext.ThuTu.IsThuTu)
        {
            score += 12;
            reasons.Add("Không Thọ Tử");
        }
        else
        {
            reasons.Add("Thọ Tử");
        }

        // 8. Evaluate rules via ActionCodeMapper (filtered by GenderScope and RuleTier)
        var actionId = actionCodeMapper.ToString(action);
        var ruleMappings = actionCodeMapper.GetRuleMappingsForAction(actionId, userGender, tier);
        var resolvedRules = ruleResolver.Resolve(ruleMappings, userGender, tier);
        
        foreach (var item in resolvedRules)
        {
            var rule = item.Rule;
            var isMandatory = item.IsMandatory;
            var profile = new UserProfile
            {
                Gender = userGender,
                LunarYOB = clientLunarYear ?? lunar.LunarYear,
                TargetYear = solarDate.Year
            };
            var lunarContext = new LunarDateContext
            {
                LunarDay = lunar.LunarDay,
                LunarMonth = lunar.LunarMonth,
                IsLeap = lunar.IsLeap,
                LunarYear = lunar.LunarYear,
                Jdn = lunar.Jdn
            };
            var context = new RuleContext
            {
                Profile = profile,
                Lunar = lunarContext
            };
            var result = rule.Evaluate(context);
            score += result.ScoreImpact;
            reasons.Add($"{rule.RuleCode}: {(result.IsPassed ? "Đạt" : "Không đạt")} ({result.ScoreImpact:+0;-0}{result.ScoreImpact})");
        }

        return new DayScoreResult(
            SolarDate: solarDate,
            LunarDateText: $"{lunar.LunarDay}/{lunar.LunarMonth} {canChiCalculator.GetCanChiNam(lunar.LunarYear).Can} {canChiCalculator.GetCanChiNam(lunar.LunarYear).Chi}",
            CanChiNgay: dayContext.CanChiNgay,
            TrucIndex: dayContext.TrucIndex,
            TrucName: dayContext.TrucName,
            TuIndex: dayContext.NhiThapBatTu.Index,
            TuName: dayContext.NhiThapBatTu.Name,
            IsHoangDao: dayContext.HoangDao.IsHoangDao,
            IsSatChu: dayContext.SatChu.IsSatChu,
            IsThuTu: dayContext.ThuTu.IsThuTu,
            IsNgayKy: dayContext.IsNgayKy,
            IsXungTuoi: isXungTuoi,
            Score: score,
            MaxScore: MaxScore,
            Reasons: reasons
        );
    }

    private static bool IsXungChi(string chi1, string chi2)
    {
        // Chi conflict pairs: Tý-Mão, Sửu-Mão, Dần-Sửu, Mão-Tý, Thìn-Sửu, Tỵ-Mão, Ngọ-Tý, Mùi-Sửu, Thân-Dần, Dậu-Mão, Tuất-Sửu, Hợi-Tý
        var conflictPairs = new HashSet<(string, string)>
        {
            ("Tý", "Mão"), ("Mão", "Tý"),
            ("Sửu", "Mão"), ("Mão", "Sửu"),
            ("Dần", "Sửu"), ("Sửu", "Dần"),
            ("Thìn", "Sửu"),
            ("Tỵ", "Mão"), ("Mão", "Tỵ"),
            ("Ngọ", "Tý"), ("Tý", "Ngọ"),
            ("Mùi", "Sửu"), ("Sửu", "Mùi"),
            ("Thân", "Dần"), ("Dần", "Thân"),
            ("Dậu", "Mão"), ("Mão", "Dậu"),
            ("Tuất", "Sửu"), ("Sửu", "Tuất"),
            ("Hợi", "Tý"), ("Tý", "Hợi")
        };
        return conflictPairs.Contains((chi1, chi2)) || conflictPairs.Contains((chi2, chi1));
    }

    private static string GetActionName(ActionCode action) => action switch
    {
        ActionCode.NHAP_TRACH => "nhập trạch",
        ActionCode.CUOI_HOI => "cưới hỏi",
        ActionCode.KHAI_TRUONG => "khai trương",
        ActionCode.XAY_NHA => "xây nhà",
        ActionCode.KY_HOP_DONG => "ký hợp đồng",
        ActionCode.XUAT_HANH => "xuất hành",
        ActionCode.TU_TUC => "tự tứ",
        ActionCode.AN_TANG => "an táng",
        ActionCode.SINH_CON => "sinh con",
        ActionCode.SUA_NHA => "sửa nhà",
        ActionCode.NHAN_VIEC => "nhận việc",
        ActionCode.MUA_VANG => "mua vàng",
        ActionCode.MUA_DAT => "mua đất",
        ActionCode.MUA_XE => "mua xe",
        ActionCode.DAM_BAO_HANH => "bảo hành",
        ActionCode.CU_HUONG => "về quê",
        ActionCode.BAT_DAU => "bắt đầu",
        ActionCode.CHUA_BENH => "chữa bệnh",
        ActionCode.TAM_SOAT => "tầm soát",
        ActionCode.KHAI_VONG => "khai võng",
        ActionCode.THI_DAU => "thi đấu",
        ActionCode.BOC_MO => "bốc mộ",
        ActionCode.THO_MAU => "thổ mộ",
        ActionCode.LE_BAI => "lễ bái",
        ActionCode.CAT_SAC => "cắt sắc",
        _ => action.ToString()
    };
}