using FluentAssertions;
using Moq;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.CanChi.Models;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Features.EvaluateSpiritualAction.Rules;

namespace ZenTam.Api.UnitTests.EvaluateSpiritualAction.Rules;

public class TamSatThangRuleTests
{
    private readonly Mock<ICanChiCalculator> _canChiCalculatorMock;

    public TamSatThangRuleTests()
    {
        _canChiCalculatorMock = new Mock<ICanChiCalculator>();
        _canChiCalculatorMock.Setup(c => c.GetCanChiThang(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(new CanChiMonth("Giáp", "Tý"));
    }

    private TamSatThangRule CreateRule() => new(_canChiCalculatorMock.Object);

    private static UserProfile DefaultProfile => new()
    {
        Gender = Gender.Male,
        LunarYOB = 1996,
        TargetYear = 2026
    };

    [Theory]
    [InlineData(1, false, -12)]  // Ngày 1 (Kiếp Sát cho Tý) → FAIL
    [InlineData(2, false, -12)]  // Ngày 2 (Tai Sát cho Tý) → FAIL
    [InlineData(3, false, -12)]  // Ngày 3 (Kiếp Sát cho Tý) → FAIL
    [InlineData(4, false, -12)]  // Ngày 4 (Tai Sát cho Tý) → FAIL
    [InlineData(5, false, -12)]  // Ngày 5 (Kiếp Sát cho Tý) → FAIL
    [InlineData(6, false, -12)]  // Ngày 6 (Tai Sát cho Tý) → FAIL
    [InlineData(7, true, 0)]     // Ngày 7 → PASS
    [InlineData(10, true, 0)]    // Ngày 10 → PASS
    [InlineData(15, true, 0)]   // Ngày 15 → PASS
    public void ThangTy_KiepSatTaiSat_FailOnCorrectDays(int lunarDay, bool expectedPassed, int expectedScore)
    {
        // Arrange: Month 1 = Tý (Chi=1), Kiếp Sát = 1,3,5; Tai Sát = 2,4,6
        _canChiCalculatorMock.Setup(c => c.GetCanChiThang(2026, 1, false))
            .Returns(new CanChiMonth("Giáp", "Tý"));

        var rule = CreateRule();
        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 1, // Tý month
            LunarDay = lunarDay,
            IsLeap = false,
            Jdn = 2460000
        };

        // Act
        var result = rule.Evaluate(DefaultProfile, context);

        // Assert
        result.RuleName.Should().Be("TAM_SAT_THANG");
        result.IsPassed.Should().Be(expectedPassed);
        result.Score.Should().Be(expectedScore);
        result.IsMandatory.Should().BeTrue();
    }

    [Theory]
    [InlineData(1, true, 0)]     // Ngày 1 → PASS (not a sat day for Dần)
    [InlineData(2, false, -12)] // Ngày 2 (Tai Sát cho Dần) → FAIL
    [InlineData(3, false, -12)] // Ngày 3 (Kiếp Sát cho Dần) → FAIL
    [InlineData(4, false, -12)] // Ngày 4 (Tai Sát cho Dần) → FAIL
    [InlineData(5, false, -12)] // Ngày 5 (Kiếp Sát cho Dần) → FAIL
    [InlineData(6, false, -12)] // Ngày 6 (Tai Sát cho Dần) → FAIL
    [InlineData(7, false, -12)] // Ngày 7 (Kiếp Sát cho Dần) → FAIL
    [InlineData(8, true, 0)]    // Ngày 8 → PASS
    [InlineData(10, true, 0)]   // Ngày 10 → PASS
    public void ThangDan_KiepSatTaiSat_FailOnCorrectDays(int lunarDay, bool expectedPassed, int expectedScore)
    {
        // Arrange: Month 3 = Dần (Chi=3), Kiếp Sát = 3,5,7; Tai Sát = 2,4,6
        _canChiCalculatorMock.Setup(c => c.GetCanChiThang(2026, 3, false))
            .Returns(new CanChiMonth("Mậu", "Dần"));

        var rule = CreateRule();
        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 3, // Dần month
            LunarDay = lunarDay,
            IsLeap = false,
            Jdn = 2460000
        };

        // Act
        var result = rule.Evaluate(DefaultProfile, context);

        // Assert
        result.RuleName.Should().Be("TAM_SAT_THANG");
        result.IsPassed.Should().Be(expectedPassed);
        result.Score.Should().Be(expectedScore);
    }

    [Theory]
    [InlineData(1, false, -12)] // Ngày 1 (Kiếp Sát cho Sửu) → FAIL
    [InlineData(2, false, -12)] // Ngày 2 (Kiếp Sát cho Sửu) → FAIL
    [InlineData(3, false, -12)] // Ngày 3 (Tai Sát cho Sửu) → FAIL
    [InlineData(4, false, -12)] // Ngày 4 (Kiếp Sát cho Sửu) → FAIL
    [InlineData(5, false, -12)] // Ngày 5 (Tai Sát cho Sửu) → FAIL
    [InlineData(6, false, -12)] // Ngày 6 (Kiếp Sát cho Sửu) → FAIL
    [InlineData(7, true, 0)]    // Ngày 7 → PASS
    public void ThangSuu_KiepSatTaiSat_FailOnCorrectDays(int lunarDay, bool expectedPassed, int expectedScore)
    {
        // Arrange: Month 2 = Sửu (Chi=2), Kiếp Sát = 2,4,6; Tai Sát = 1,3,5
        _canChiCalculatorMock.Setup(c => c.GetCanChiThang(2026, 2, false))
            .Returns(new CanChiMonth("Ất", "Sửu"));

        var rule = CreateRule();
        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 2, // Sửu month
            LunarDay = lunarDay,
            IsLeap = false,
            Jdn = 2460000
        };

        // Act
        var result = rule.Evaluate(DefaultProfile, context);

        // Assert
        result.RuleName.Should().Be("TAM_SAT_THANG");
        result.IsPassed.Should().Be(expectedPassed);
        result.Score.Should().Be(expectedScore);
    }

    [Fact]
    public void ThangNgo_KiepSatTaiSat_AllSatDaysFail()
    {
        // Arrange: Month 7 = Ngọ (Chi=7), same as Tý group: Kiếp Sát = 1,3,5; Tai Sát = 2,4,6
        _canChiCalculatorMock.Setup(c => c.GetCanChiThang(2026, 7, false))
            .Returns(new CanChiMonth("Bính", "Ngọ"));

        var rule = CreateRule();

        // Days 1,2,3,4,5,6 should fail
        foreach (var day in new[] { 1, 2, 3, 4, 5, 6 })
        {
            var context = new LunarDateContext
            {
                LunarYear = 2026,
                LunarMonth = 7, // Ngọ month
                LunarDay = day,
                IsLeap = false,
                Jdn = 2460000
            };

            var result = rule.Evaluate(DefaultProfile, context);
            result.IsPassed.Should().BeFalse($"Day {day} should fail for Tháng Ngọ");
            result.Score.Should().Be(-12);
        }

        // Days 7, 8, 9, 10 should pass
        foreach (var day in new[] { 7, 8, 9, 10, 15, 20 })
        {
            var context = new LunarDateContext
            {
                LunarYear = 2026,
                LunarMonth = 7, // Ngọ month
                LunarDay = day,
                IsLeap = false,
                Jdn = 2460000
            };

            var result = rule.Evaluate(DefaultProfile, context);
            result.IsPassed.Should().BeTrue($"Day {day} should pass for Tháng Ngọ");
            result.Score.Should().Be(0);
        }
    }

    [Fact]
    public void ThangHit_KiepSatTaiSat_AllSatDaysFail()
    {
        // Arrange: Month 12 = Hợi (Chi=12), same as Dần group: Kiếp Sát = 3,5,7; Tai Sát = 2,4,6
        _canChiCalculatorMock.Setup(c => c.GetCanChiThang(2026, 12, false))
            .Returns(new CanChiMonth("Đinh", "Hợi"));

        var rule = CreateRule();

        // Days 2,3,4,5,6,7 should fail
        foreach (var day in new[] { 2, 3, 4, 5, 6, 7 })
        {
            var context = new LunarDateContext
            {
                LunarYear = 2026,
                LunarMonth = 12, // Hợi month
                LunarDay = day,
                IsLeap = false,
                Jdn = 2460000
            };

            var result = rule.Evaluate(DefaultProfile, context);
            result.IsPassed.Should().BeFalse($"Day {day} should fail for Tháng Hợi");
            result.Score.Should().Be(-12);
        }

        // Days 1, 8, 9, 10, 11 should pass
        foreach (var day in new[] { 1, 8, 9, 10, 11, 15, 20, 30 })
        {
            var context = new LunarDateContext
            {
                LunarYear = 2026,
                LunarMonth = 12, // Hợi month
                LunarDay = day,
                IsLeap = false,
                Jdn = 2460000
            };

            var result = rule.Evaluate(DefaultProfile, context);
            result.IsPassed.Should().BeTrue($"Day {day} should pass for Tháng Hợi");
            result.Score.Should().Be(0);
        }
    }

    [Fact]
    public void Evaluate_ReturnsSpecificFailMessageForSatDay()
    {
        // Arrange
        _canChiCalculatorMock.Setup(c => c.GetCanChiThang(2026, 3, false))
            .Returns(new CanChiMonth("Mậu", "Dần"));

        var rule = CreateRule();
        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 3, // Dần month
            LunarDay = 3,   // Kiếp Sát day
            IsLeap = false,
            Jdn = 2460000
        };

        // Act
        var result = rule.Evaluate(DefaultProfile, context);

        // Assert
        result.IsPassed.Should().BeFalse();
        result.Message.Should().Contain("Phạm Tam Sát Tháng");
        result.Message.Should().Contain("ngày 3");
        result.Message.Should().Contain("tháng 3");
    }

    [Fact]
    public void Evaluate_ReturnsPassMessageForNonSatDay()
    {
        // Arrange
        _canChiCalculatorMock.Setup(c => c.GetCanChiThang(2026, 4, false))
            .Returns(new CanChiMonth("Kỷ", "Mão"));

        var rule = CreateRule();
        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 4, // Mão month
            LunarDay = 10,  // Not a sat day
            IsLeap = false,
            Jdn = 2460000
        };

        // Act
        var result = rule.Evaluate(DefaultProfile, context);

        // Assert
        result.IsPassed.Should().BeTrue();
        result.Score.Should().Be(0);
        result.Message.Should().Contain("Không phạm Tam Sát Tháng");
    }
}