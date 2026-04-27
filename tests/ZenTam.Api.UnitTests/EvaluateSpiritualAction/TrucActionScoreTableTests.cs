using FluentAssertions;
using ZenTam.Api.Features.Calendars.Models;
using ZenTam.Api.Features.EvaluateSpiritualAction.Data;
using ZenTam.Api.Features.EvaluateSpiritualAction.Models;

namespace ZenTam.Api.UnitTests.EvaluateSpiritualAction;

/// <summary>
/// Tests for TrucActionScoreTable - the 12x12 Truc-Action scoring matrix.
/// Row = TrucIndex (0-11), Col = ActionIndex (0-11).
/// </summary>
public class TrucActionScoreTableTests
{
    [Fact]
    public void GetScore_ThanhKienKetHon_Returns20()
    {
        // Arrange
        var truc = TrucType.Thành; // Index 8
        var action = ActionCode.KET_HON; // Index 1

        // Act
        var score = TrucActionScoreTable.GetScore(truc, action);

        // Assert
        score.Should().Be(20);
    }

    [Fact]
    public void GetScore_NguyKienKetHon_ReturnsNegative30()
    {
        // Arrange
        var truc = TrucType.Nguy; // Index 7
        var action = ActionCode.KET_HON; // Index 1

        // Act
        var score = TrucActionScoreTable.GetScore(truc, action);

        // Assert
        score.Should().Be(-30);
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
        // Act & Assert - all 12 Truc values should be valid
        var action = ActionCode.NHAP_TRACH;
        var score = TrucActionScoreTable.GetScore(truc, action);
        score.Should().BeInRange(-30, 20);
    }

    [Theory]
    [InlineData(ActionCode.NHAP_TRACH)]    // 0
    [InlineData(ActionCode.KET_HON)]       // 1
    [InlineData(ActionCode.KHAI_TRUONG)]   // 2
    [InlineData(ActionCode.DONG_THO)]      // 3
    [InlineData(ActionCode.KY_HOP_DONG)]   // 4
    [InlineData(ActionCode.XUAT_HANH)]     // 5
    [InlineData(ActionCode.TU_TUC)]        // 6
    [InlineData(ActionCode.CUA_HANG)]      // 7
    [InlineData(ActionCode.AN_TANG)]       // 8
    [InlineData(ActionCode.TAM_TRIEN)]    // 9
    [InlineData(ActionCode.KHAI_NGHIEP)]   // 10
    [InlineData(ActionCode.CHUA_BENH)]     // 11
    public void GetScore_AllActionIndices_ReturnValid(ActionCode action)
    {
        // Act & Assert - all 12 Action values should be valid
        var truc = TrucType.Kiến;
        var score = TrucActionScoreTable.GetScore(truc, action);
        score.Should().BeInRange(-30, 20);
    }

    [Fact]
    public void GetScore_All144Cells_HaveValidRange()
    {
        // The table is 12 rows (Truc) x 12 cols (Action) = 144 cells
        // Each cell should be between -30 and 20

        for (int trucIdx = 0; trucIdx < 12; trucIdx++)
        {
            for (int actionIdx = 0; actionIdx < 12; actionIdx++)
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
        // Thành (index 8) + NHAP_TRACH (index 0) = 20 (best case)
        var score = TrucActionScoreTable.GetScore(TrucType.Thành, ActionCode.NHAP_TRACH);
        score.Should().Be(20);
    }

    [Fact]
    public void GetScore_ThanhKiTuTuc_ReturnsNegative10()
    {
        // Thành (index 8) + TU_TUC (index 6) = -10 (exception for this action)
        var score = TrucActionScoreTable.GetScore(TrucType.Thành, ActionCode.TU_TUC);
        score.Should().Be(-10);
    }

    [Fact]
    public void GetScore_PhaDongTho_Returns12()
    {
        // Phá (index 6) + DONG_THO (index 3) = 12 (exception)
        var score = TrucActionScoreTable.GetScore(TrucType.Phá, ActionCode.DONG_THO);
        score.Should().Be(12);
    }

    [Fact]
    public void GetScore_PhaAnTang_ReturnsNegative10()
    {
        // Phá (index 6) + AN_TANG (index 8) = -10
        var score = TrucActionScoreTable.GetScore(TrucType.Phá, ActionCode.AN_TANG);
        score.Should().Be(-10);
    }

    [Fact]
    public void GetScore_BeAnTang_Returns12()
    {
        // Bế (index 11) + AN_TANG (index 8) = 12 (exception)
        var score = TrucActionScoreTable.GetScore(TrucType.Bế, ActionCode.AN_TANG);
        score.Should().Be(12);
    }

    [Fact]
    public void GetScore_KienDongTho_ReturnsNegative10()
    {
        // Kiến (index 0) + DONG_THO (index 3) = -10
        var score = TrucActionScoreTable.GetScore(TrucType.Kiến, ActionCode.DONG_THO);
        score.Should().Be(-10);
    }
}