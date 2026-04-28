using FluentAssertions;
using ZenTam.Api.Features.Calendars.Models;
using ZenTam.Api.Features.EvaluateSpiritualAction.Data;
using ZenTam.Api.Features.EvaluateSpiritualAction.Models;

namespace ZenTam.Api.UnitTests.EvaluateSpiritualAction;

/// <summary>
/// Tests for TrucActionScoreTable - the 12x25 Truc-Action scoring matrix.
/// Row = TrucIndex (0-11), Col = ActionIndex (0-24, total 25 actions).
/// </summary>
public class TrucActionScoreTableTests
{
    // Action indices (from ActionCode enum):
    //  0: XAY_NHA     1: SUA_NHA     2: NHAP_TRACH  3: CUOI_HOI    4: SINH_CON
    //  5: KHAI_TRUONG 6: KY_HOP_DONG 7: NHAN_VIEC   8: MUA_VANG    9: MUA_DAT
    // 10: MUA_XE     11: DAM_BAO_HANH 12: XUAT_HANH 13: CU_HUONG   14: BAT_DAU
    // 15: CHUA_BENH  16: TAM_SOAT    17: KHAI_VONG  18: THI_DAU    19: AN_TANG
    // 20: BOC_MO     21: THO_MAU     22: LE_BAI     23: CAT_SAC    24: TU_TUC

    [Fact]
    public void GetScore_ThanhKienKetHon_Returns20()
    {
        // Arrange - Thành (index 8) + CUOI_HOI (index 3)
        var truc = TrucType.Thành; // Index 8
        var action = ActionCode.CUOI_HOI; // Index 3

        // Act
        var score = TrucActionScoreTable.GetScore(truc, action);

        // Assert - Row 8 (Thành), Col 3 = 20
        score.Should().Be(20);
    }

    [Fact]
    public void GetScore_NguyKienKetHon_ReturnsNegative20()
    {
        // Arrange - Nguy (index 7) + CUOI_HOI (table col 22)
        var truc = TrucType.Nguy; // Index 7
        var action = ActionCode.CUOI_HOI; // Index 3

        // Act
        var score = TrucActionScoreTable.GetScore(truc, action);

        // Assert - Row 7 (Nguy), Col 22 (CUOI_HOI in table ordering) = -20
        score.Should().Be(-20);
    }

    [Theory]
    [InlineData(TrucType.Kiến)]   // 0
    [InlineData(TrucType.Trừ)]    // 1
    [InlineData(TrucType.Mãn)]    // 2
    [InlineData(TrucType.Bình)]   // 3
    [InlineData(TrucType.Định)]   // 4
    [InlineData(TrucType.Chấp)]   // 5
    [InlineData(TrucType.Phá)]    // 6
    [InlineData(TrucType.Nguy)]   // 7
    [InlineData(TrucType.Thành)]  // 8
    [InlineData(TrucType.Thu)]    // 9
    [InlineData(TrucType.Khai)]   // 10
    [InlineData(TrucType.Bế)]     // 11
    public void GetScore_AllTrucIndices_ReturnValid(TrucType truc)
    {
        // Act & Assert - all 12 Truc values should be valid for all 25 actions
        var action = ActionCode.NHAP_TRACH; // index 2
        var score = TrucActionScoreTable.GetScore(truc, action);
        score.Should().BeInRange(-30, 20);
    }

    [Theory]
    [InlineData(ActionCode.XAY_NHA)]     // 0
    [InlineData(ActionCode.SUA_NHA)]    // 1
    [InlineData(ActionCode.NHAP_TRACH)]  // 2
    [InlineData(ActionCode.CUOI_HOI)]    // 3
    [InlineData(ActionCode.SINH_CON)]    // 4
    [InlineData(ActionCode.KHAI_TRUONG)] // 5
    [InlineData(ActionCode.KY_HOP_DONG)] // 6
    [InlineData(ActionCode.NHAN_VIEC)]    // 7
    [InlineData(ActionCode.MUA_VANG)]    // 8
    [InlineData(ActionCode.MUA_DAT)]     // 9
    [InlineData(ActionCode.MUA_XE)]      // 10
    [InlineData(ActionCode.DAM_BAO_HANH)]// 11
    [InlineData(ActionCode.XUAT_HANH)]   // 12
    [InlineData(ActionCode.CU_HUONG)]     // 13
    [InlineData(ActionCode.BAT_DAU)]     // 14
    [InlineData(ActionCode.CHUA_BENH)]   // 15
    [InlineData(ActionCode.TAM_SOAT)]    // 16
    [InlineData(ActionCode.KHAI_VONG)]   // 17
    [InlineData(ActionCode.THI_DAU)]     // 18
    [InlineData(ActionCode.AN_TANG)]     // 19
    [InlineData(ActionCode.BOC_MO)]       // 20
    [InlineData(ActionCode.THO_MAU)]     // 21
    [InlineData(ActionCode.LE_BAI)]      // 22
    [InlineData(ActionCode.CAT_SAC)]     // 23
    [InlineData(ActionCode.TU_TUC)]      // 24
    public void GetScore_AllActionIndices_ReturnValid(ActionCode action)
    {
        // Act & Assert - all 25 Action values should be valid
        var truc = TrucType.Kiến;
        var score = TrucActionScoreTable.GetScore(truc, action);
        score.Should().BeInRange(-30, 20);
    }

    [Fact]
    public void GetScore_All300Cells_HaveValidRange()
    {
        // The table is 12 rows (Truc) x 25 cols (Action) = 300 cells
        // Each cell should be between -30 and 20

        for (int trucIdx = 0; trucIdx < 12; trucIdx++)
        {
            for (int actionIdx = 0; actionIdx < 25; actionIdx++)
            {
                var score = TrucActionScoreTable.GetScore((TrucType)trucIdx, (ActionCode)actionIdx);
                score.Should().BeInRange(-30, 20,
                    $"Score at row={trucIdx}, col={actionIdx} should be between -30 and 20");
            }
        }
    }

    [Fact]
    public void GetScore_ThanhKiNhapTrach_Returns20()
    {
        // Thành (index 8) + NHAP_TRACH (index 2) = 20 (best case)
        var score = TrucActionScoreTable.GetScore(TrucType.Thành, ActionCode.NHAP_TRACH);
        score.Should().Be(20);
    }

    [Fact]
    public void GetScore_ThanhKiTuTuc_Returns20()
    {
        // Thành (index 8) + TU_TUC (table col 20) = 20
        var score = TrucActionScoreTable.GetScore(TrucType.Thành, ActionCode.TU_TUC);
        score.Should().Be(20);
    }

    [Fact]
    public void GetScore_PhaNhanViec_ReturnsNegative15()
    {
        // Phá (index 6) + NHAN_VIEC (index 7) = -15
        var score = TrucActionScoreTable.GetScore(TrucType.Phá, ActionCode.NHAN_VIEC);
        score.Should().Be(-15);
    }

    [Fact]
    public void GetScore_PhaAnTang_ReturnsNegative15()
    {
        // Phá (index 6) + AN_TANG (table col 15) = -15
        var score = TrucActionScoreTable.GetScore(TrucType.Phá, ActionCode.AN_TANG);
        score.Should().Be(-15);
    }

    [Fact]
    public void GetScore_BeAnTang_ReturnsNegative15()
    {
        // Bế (index 11) + AN_TANG (table col 15) = -15
        var score = TrucActionScoreTable.GetScore(TrucType.Bế, ActionCode.AN_TANG);
        score.Should().Be(-15);
    }

    [Fact]
    public void GetScore_KienNhanViec_Returns12()
    {
        // Kiến (index 0) + NHAN_VIEC (index 7) = 12
        var score = TrucActionScoreTable.GetScore(TrucType.Kiến, ActionCode.NHAN_VIEC);
        score.Should().Be(12);
    }

    [Fact]
    public void GetScore_KienXayNha_Returns12()
    {
        // Kiến (index 0) + XAY_NHA (index 0) = 12
        var score = TrucActionScoreTable.GetScore(TrucType.Kiến, ActionCode.XAY_NHA);
        score.Should().Be(12);
    }

    [Fact]
    public void GetScore_ThanhXayNha_Returns20()
    {
        // Thành (index 8) + XAY_NHA (index 0) = 20
        var score = TrucActionScoreTable.GetScore(TrucType.Thành, ActionCode.XAY_NHA);
        score.Should().Be(20);
    }

    [Fact]
    public void GetScore_NguyXayNha_ReturnsNegative20()
    {
        // Nguy (index 7) + XAY_NHA (index 0) = -20
        var score = TrucActionScoreTable.GetScore(TrucType.Nguy, ActionCode.XAY_NHA);
        score.Should().Be(-20);
    }

    [Fact]
    public void GetScore_ThanhCuoiHoi_Returns20()
    {
        // Thành (index 8) + CUOI_HOI (index 3) = 20
        var score = TrucActionScoreTable.GetScore(TrucType.Thành, ActionCode.CUOI_HOI);
        score.Should().Be(20);
    }

    [Fact]
    public void GetScore_ThanhAnTang_Returns20()
    {
        // Thành (index 8) + AN_TANG (table col 15) = 20
        var score = TrucActionScoreTable.GetScore(TrucType.Thành, ActionCode.AN_TANG);
        score.Should().Be(20);
    }

    [Fact]
    public void GetScore_KhaiKhaiTruong_Returns18()
    {
        // Khai (index 10) + KHAI_TRUONG (index 5) = 18
        var score = TrucActionScoreTable.GetScore(TrucType.Khai, ActionCode.KHAI_TRUONG);
        score.Should().Be(18);
    }
}