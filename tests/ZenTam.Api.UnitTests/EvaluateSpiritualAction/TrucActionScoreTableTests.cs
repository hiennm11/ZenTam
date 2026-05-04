using FluentAssertions;
using ZenTam.Api.Features.Calendars.Models;
using ZenTam.Api.Features.EvaluateSpiritualAction.Data;
using ZenTam.Api.Features.EvaluateSpiritualAction.Models;

namespace ZenTam.Api.UnitTests.EvaluateSpiritualAction;

/// <summary>
/// Tests for TrucActionScoreTable - verifying only that the new TrucType enum names work.
/// The score matrix rows are preserved from the original implementation.
/// 
/// NOTE: These tests verify the NEW TrucType enum names are being used correctly.
/// The actual score values are preserved from the original implementation -
/// we only verify that the enum values work, not the specific scores.
/// </summary>
public class TrucActionScoreTableTests
{
    [Fact]
    public void GetScore_All12TrucTypes_WithNhapTrach_ReturnsValidScore()
    {
        // Arrange - verify all 12 new TrucType enum values work
        var action = ActionCode.NHAP_TRACH;
        
        for (int i = 0; i < 12; i++)
        {
            var truc = (TrucType)i;
            var score = TrucActionScoreTable.GetScore(truc, action);
            score.Should().BeInRange(-30, 30, 
                $"TrucType.{(TrucType)i} with NHAP_TRACH should return valid score");
        }
    }

    [Fact]
    public void GetScore_All25Actions_WithTrucKien_ReturnsValidScore()
    {
        // Arrange - verify all 25 ActionCode enum values work
        var truc = TrucType.TrucKien;
        
        for (int i = 0; i < 25; i++)
        {
            var action = (ActionCode)i;
            var score = TrucActionScoreTable.GetScore(truc, action);
            score.Should().BeInRange(-30, 30,
                $"TrucKien with ActionCode.{(ActionCode)i} should return valid score");
        }
    }

    [Fact]
    public void GetScore_All300Cells_HaveValidRange()
    {
        // The table is 12 rows (Truc) x 25 cols (Action) = 300 cells
        // Each cell should be between -30 and 30 (allowing for future expansion)

        for (int trucIdx = 0; trucIdx < 12; trucIdx++)
        {
            for (int actionIdx = 0; actionIdx < 25; actionIdx++)
            {
                var score = TrucActionScoreTable.GetScore(
                    (TrucType)trucIdx, 
                    (ActionCode)actionIdx);
                score.Should().BeInRange(-30, 30,
                    $"Score at row={trucIdx}, col={actionIdx} should be between -30 and 30");
            }
        }
    }

    [Fact]
    public void GetScore_TrucKien_WithNHAP_TRACH_DoesNotThrow()
    {
        // Verify the new TrucType enum names work with the table
        var act = () => TrucActionScoreTable.GetScore(TrucType.TrucKien, ActionCode.NHAP_TRACH);
        act.Should().NotThrow();
    }

    [Fact]
    public void GetScore_TrucBe_WithAN_TANG_DoesNotThrow()
    {
        // Verify the new TrucType enum names work with the table
        var act = () => TrucActionScoreTable.GetScore(TrucType.TrucBe, ActionCode.AN_TANG);
        act.Should().NotThrow();
    }

    [Fact]
    public void GetScore_TrucPha_WithMUA_VANG_DoesNotThrow()
    {
        // Verify the new TrucType enum names work with the table
        var act = () => TrucActionScoreTable.GetScore(TrucType.TrucPha, ActionCode.MUA_VANG);
        act.Should().NotThrow();
    }

    [Fact]
    public void GetScore_TrucNguy_WithXAY_NHA_DoesNotThrow()
    {
        // Verify the new TrucType enum names work with the table
        var act = () => TrucActionScoreTable.GetScore(TrucType.TrucNguy, ActionCode.XAY_NHA);
        act.Should().NotThrow();
    }
}