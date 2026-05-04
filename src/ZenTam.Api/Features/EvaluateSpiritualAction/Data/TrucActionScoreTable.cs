using ZenTam.Api.Features.Calendars.Models;
using ZenTam.Api.Features.EvaluateSpiritualAction.Models;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Data;

public static class TrucActionScoreTable
{
    // Row = TrucIndex (0-11), Col = ActionIndex (0-24, total 25 actions)
    // Actions mapping:
    //  0: NHAP_TRACH    1: SUA_NHA      2: NHAN_VIEC     3: MUA_VANG      4: MUA_DAT
    //  5: KHAI_TRUONG   6: KY_HOP_DONG  7: DAM_BAO_HANH  8: XUAT_HANH     9: CU_HUONG
    // 10: BAT_DAU      11: CHUA_BENH   12: TAM_SOAT     13: KHAI_VONG    14: THI_DAU
    // 15: AN_TANG      16: BOC_MO      17: THO_MAU      18: LE_BAI       19: CAT_SAC
    // 20: TU_TUC       21: XAY_NHA     22: CUOI_HOI     23: SINH_CON     24: MUA_XE
    private static readonly int[,] Scores =
    {
        // Row 0 (TrucKien / Trực Kiến): [NHAP_TRACH, SUA_NHA, NHAN_VIEC, MUA_VANG, MUA_DAT, KHAI_TRUONG, KY_HOP_DONG, DAM_BAO_HANH, XUAT_HANH, CU_HUONG, BAT_DAU, CHUA_BENH, TAM_SOAT, KHAI_VONG, THI_DAU, AN_TANG, BOC_MO, THO_MAU, LE_BAI, CAT_SAC, TU_TUC, XAY_NHA, CUOI_HOI, SINH_CON, MUA_XE]
        { 12, 12, -10, 12, 12, 12, 12, 12, 12, 12, 12, -10, 6, 12, 12, -10, -10, -10, 12, 12, 6, 12, 12, 12, 12 },
        // Row 1 (TrucTru / Trực Trừ):
        { 12, 12, 12, 12, 12, 12, -10, 12, 6, 12, 6, 12, 12, 12, 12, -10, -10, -10, 12, 12, 12, 12, -10, 12, 12 },
        // Row 2 (TrucMan / Trực Mãn):
        { 6, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, -10, 6, 12, 12, -10, -10, -10, 12, 12, 6, 12, 12, 12, 12 },
        // Row 3 (TrucBinh / Trực Bình):
        { 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 12, 12, 12, 6, 6, 6, 6, 6, 6, 6 },
        // Row 4 (TrucDinh / Trực Định):
        { 6, 6, 12, 12, 12, 12, 12, 12, -10, 12, 6, -10, 12, 12, 12, -10, -10, -10, 12, 12, 6, 12, 6, 12, 12 },
        // Row 5 (TrucChap / Trực Chấp):
        { 6, 6, 6, 6, 6, 6, 6, 6, -10, -10, 6, -10, 6, 6, 6, -10, -10, -10, 6, 6, 6, 6, 6, 6, 6 },
        // Row 6 (TrucPha / Trực Phá):
        { -15, -15, 12, -15, -15, -15, -15, -15, -15, -15, -15, 12, -15, -15, -15, -10, -10, -10, -15, -15, -15, -15, -15, -15, -15 },
        // Row 7 (TrucNguy / Trực Nguy):
        { -20, -20, -20, -20, -20, -20, -20, -20, -20, -20, -20, -15, -20, -20, -20, -15, -15, -15, -20, -20, -20, -20, -30, -20, -20 },
        // Row 8 (TrucThanh / Trực Thành):
        { 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, -10, 20, 20, 20, -10, -10, -10, 20, 20, -10, 20, 20, 20, 20 },
        // Row 9 (TrucThu / Trực Thu):
        { 18, 18, 18, 18, 18, 6, 18, 6, -10, 6, -10, 18, 6, 18, 6, 18, 18, 18, 18, 6, -10, 18, 18, 18, 18 },
        // Row 10 (TrucKhai / Trực Khai):
        { 18, 18, -10, 18, -10, 18, -10, 18, 18, 18, -10, -10, 18, 18, 18, -10, -10, -10, 18, 18, -10, 18, 18, 18, 18 },
        // Row 11 (TrucBe / Trực Bế):
        { -15, -15, -15, -15, -15, -15, -15, -15, -15, -15, -15, -15, -15, -15, -15, 12, 12, 12, -15, -15, -15, -15, -15, -15, -15 }
    };

    public static int GetScore(TrucType truc, ActionCode action) =>
        Scores[(int)truc, (int)action];
}