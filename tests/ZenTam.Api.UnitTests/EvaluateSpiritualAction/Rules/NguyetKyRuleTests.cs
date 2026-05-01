using FluentAssertions;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Features.EvaluateSpiritualAction.Rules;

namespace ZenTam.Api.UnitTests.EvaluateSpiritualAction.Rules;

public class NguyetKyRuleTests
{
    private readonly NguyetKyRule _rule = new();

    private static UserProfile DefaultProfile => new()
    {
        Gender = Gender.Male,
        LunarYOB = 1996,
        TargetYear = 2026
    };

    [Theory]
    [InlineData(5, false, -10)]   // Ngày 5 → FAIL
    [InlineData(14, false, -10)]   // Ngày 14 → FAIL
    [InlineData(23, false, -10)]   // Ngày 23 → FAIL
    [InlineData(10, true, 0)]      // Ngày 10 → PASS
    [InlineData(1, true, 0)]       // Ngày 1 → PASS
    [InlineData(30, true, 0)]      // Ngày 30 (cuối tháng) → PASS
    [InlineData(15, true, 0)]      // Ngày 15 → PASS
    public void Evaluate_ReturnsCorrectResult(int lunarDay, bool expectedPassed, int expectedScore)
    {
        // Arrange
        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 4,
            LunarDay = lunarDay,
            IsLeap = false,
            Jdn = 2460000
        };

        // Act
        var result = _rule.Evaluate(DefaultProfile, context);

        // Assert
        result.RuleName.Should().Be("NGUYET_KY");
        result.IsPassed.Should().Be(expectedPassed);
        result.Score.Should().Be(expectedScore);
        result.IsMandatory.Should().BeFalse(); // NguyetKy is not mandatory
    }

    [Fact]
    public void Evaluate_WhenDayIs5_ReturnsSpecificFailMessage()
    {
        // Arrange
        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 4,
            LunarDay = 5,
            IsLeap = false,
            Jdn = 2460000
        };

        // Act
        var result = _rule.Evaluate(DefaultProfile, context);

        // Assert
        result.IsPassed.Should().BeFalse();
        result.Message.Should().Contain("Phạm Nguyệt Kỵ");
        result.Message.Should().Contain("mùng 5");
    }

    [Fact]
    public void Evaluate_WhenDayIs14_ReturnsSpecificFailMessage()
    {
        // Arrange
        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 4,
            LunarDay = 14,
            IsLeap = false,
            Jdn = 2460000
        };

        // Act
        var result = _rule.Evaluate(DefaultProfile, context);

        // Assert
        result.IsPassed.Should().BeFalse();
        result.Message.Should().Contain("Phạm Nguyệt Kỵ");
        result.Message.Should().Contain("mùng 14");
    }

    [Fact]
    public void Evaluate_WhenDayIs23_ReturnsSpecificFailMessage()
    {
        // Arrange
        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 4,
            LunarDay = 23,
            IsLeap = false,
            Jdn = 2460000
        };

        // Act
        var result = _rule.Evaluate(DefaultProfile, context);

        // Assert
        result.IsPassed.Should().BeFalse();
        result.Message.Should().Contain("Phạm Nguyệt Kỵ");
        result.Message.Should().Contain("mùng 23");
    }

    [Fact]
    public void Evaluate_WhenDayIsSafe_ReturnsPassMessage()
    {
        // Arrange
        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 4,
            LunarDay = 10,
            IsLeap = false,
            Jdn = 2460000
        };

        // Act
        var result = _rule.Evaluate(DefaultProfile, context);

        // Assert
        result.IsPassed.Should().BeTrue();
        result.Score.Should().Be(0);
        result.Message.Should().Contain("Không phạm Nguyệt Kỵ");
        result.Message.Should().Contain("ngày 10");
    }
}