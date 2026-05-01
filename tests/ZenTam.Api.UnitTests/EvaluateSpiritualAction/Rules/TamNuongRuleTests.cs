using FluentAssertions;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Features.EvaluateSpiritualAction.Rules;

namespace ZenTam.Api.UnitTests.EvaluateSpiritualAction.Rules;

public class TamNuongRuleTests
{
    private readonly TamNuongRule _rule = new();

    private static UserProfile DefaultProfile => new()
    {
        Gender = Gender.Male,
        LunarYOB = 1996,
        TargetYear = 2026
    };

    [Theory]
    [InlineData(3, false, -8)]    // Ngày 3 → FAIL
    [InlineData(7, false, -8)]     // Ngày 7 → FAIL
    [InlineData(13, false, -8)]   // Ngày 13 → FAIL
    [InlineData(18, false, -8)]   // Ngày 18 → FAIL
    [InlineData(22, false, -8)]   // Ngày 22 → FAIL
    [InlineData(27, false, -8)]   // Ngày 27 → FAIL
    [InlineData(10, true, 0)]      // Ngày 10 → PASS
    [InlineData(1, true, 0)]       // Ngày 1 → PASS
    [InlineData(30, true, 0)]     // Ngày 30 → PASS
    [InlineData(15, true, 0)]     // Ngày 15 → PASS
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
        result.RuleName.Should().Be("TAM_NUONG");
        result.IsPassed.Should().Be(expectedPassed);
        result.Score.Should().Be(expectedScore);
        result.IsMandatory.Should().BeFalse(); // TamNuong is not mandatory
    }

    [Fact]
    public void Evaluate_WhenDayIs3_ReturnsSpecificFailMessage()
    {
        // Arrange
        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 4,
            LunarDay = 3,
            IsLeap = false,
            Jdn = 2460000
        };

        // Act
        var result = _rule.Evaluate(DefaultProfile, context);

        // Assert
        result.IsPassed.Should().BeFalse();
        result.Message.Should().Contain("Phạm Tam Nuông");
        result.Message.Should().Contain("mùng 3");
    }

    [Fact]
    public void Evaluate_WhenDayIs7_ReturnsSpecificFailMessage()
    {
        // Arrange
        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 4,
            LunarDay = 7,
            IsLeap = false,
            Jdn = 2460000
        };

        // Act
        var result = _rule.Evaluate(DefaultProfile, context);

        // Assert
        result.IsPassed.Should().BeFalse();
        result.Message.Should().Contain("Phạm Tam Nuông");
        result.Message.Should().Contain("mùng 7");
    }

    [Fact]
    public void Evaluate_WhenDayIs13_ReturnsSpecificFailMessage()
    {
        // Arrange
        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 4,
            LunarDay = 13,
            IsLeap = false,
            Jdn = 2460000
        };

        // Act
        var result = _rule.Evaluate(DefaultProfile, context);

        // Assert
        result.IsPassed.Should().BeFalse();
        result.Message.Should().Contain("Phạm Tam Nuông");
        result.Message.Should().Contain("mùng 13");
    }

    [Fact]
    public void Evaluate_WhenDayIs18_ReturnsSpecificFailMessage()
    {
        // Arrange
        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 4,
            LunarDay = 18,
            IsLeap = false,
            Jdn = 2460000
        };

        // Act
        var result = _rule.Evaluate(DefaultProfile, context);

        // Assert
        result.IsPassed.Should().BeFalse();
        result.Message.Should().Contain("Phạm Tam Nuông");
        result.Message.Should().Contain("mùng 18");
    }

    [Fact]
    public void Evaluate_WhenDayIs22_ReturnsSpecificFailMessage()
    {
        // Arrange
        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 4,
            LunarDay = 22,
            IsLeap = false,
            Jdn = 2460000
        };

        // Act
        var result = _rule.Evaluate(DefaultProfile, context);

        // Assert
        result.IsPassed.Should().BeFalse();
        result.Message.Should().Contain("Phạm Tam Nuông");
        result.Message.Should().Contain("mùng 22");
    }

    [Fact]
    public void Evaluate_WhenDayIs27_ReturnsSpecificFailMessage()
    {
        // Arrange
        var context = new LunarDateContext
        {
            LunarYear = 2026,
            LunarMonth = 4,
            LunarDay = 27,
            IsLeap = false,
            Jdn = 2460000
        };

        // Act
        var result = _rule.Evaluate(DefaultProfile, context);

        // Assert
        result.IsPassed.Should().BeFalse();
        result.Message.Should().Contain("Phạm Tam Nuông");
        result.Message.Should().Contain("mùng 27");
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
        result.Message.Should().Contain("Không phạm Tam Nuông");
        result.Message.Should().Contain("ngày 10");
    }
}