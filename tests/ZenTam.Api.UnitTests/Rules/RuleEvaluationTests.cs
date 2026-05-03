using FluentAssertions;
using ZenTam.Api.Common.Rules.Models;

namespace ZenTam.Api.UnitTests.Rules;

public class RuleEvaluationTests
{
    [Fact]
    public void Constructor_WithAllRequiredProperties_SetsProperties()
    {
        // Arrange & Act
        var evaluation = new RuleEvaluation
        {
            RuleCode = "NGUYET_KY",
            IsPassed = false,
            ScoreImpact = -10,
            IsMandatory = true,
            Severity = RuleSeverity.DaiHung,
            IsBlocked = true,
            Message = "Phạm Nguyệt Kỵ"
        };

        // Assert
        evaluation.RuleCode.Should().Be("NGUYET_KY");
        evaluation.IsPassed.Should().BeFalse();
        evaluation.ScoreImpact.Should().Be(-10);
        evaluation.IsMandatory.Should().BeTrue();
        evaluation.Severity.Should().Be(RuleSeverity.DaiHung);
        evaluation.IsBlocked.Should().BeTrue();
        evaluation.Message.Should().Be("Phạm Nguyệt Kỵ");
    }

    [Fact]
    public void Equality_SameValues_AreEqualByPropertyComparison()
    {
        // Arrange
        var eval1 = new RuleEvaluation
        {
            RuleCode = "NGUYET_KY",
            IsPassed = false,
            ScoreImpact = -10,
            IsMandatory = true,
            Severity = RuleSeverity.DaiHung,
            IsBlocked = true,
            Message = "Phạm Nguyệt Kỵ"
        };

        var eval2 = new RuleEvaluation
        {
            RuleCode = "NGUYET_KY",
            IsPassed = false,
            ScoreImpact = -10,
            IsMandatory = true,
            Severity = RuleSeverity.DaiHung,
            IsBlocked = true,
            Message = "Phạm Nguyệt Kỵ"
        };

        // Act & Assert - Verify all properties match (value equality)
        eval1.RuleCode.Should().Be(eval2.RuleCode);
        eval1.IsPassed.Should().Be(eval2.IsPassed);
        eval1.ScoreImpact.Should().Be(eval2.ScoreImpact);
        eval1.IsMandatory.Should().Be(eval2.IsMandatory);
        eval1.Severity.Should().Be(eval2.Severity);
        eval1.IsBlocked.Should().Be(eval2.IsBlocked);
        eval1.Message.Should().Be(eval2.Message);
    }

    [Fact]
    public void Inequality_DifferentRuleCode_AreNotEqual()
    {
        // Arrange
        var eval1 = new RuleEvaluation
        {
            RuleCode = "NGUYET_KY",
            IsPassed = false,
            ScoreImpact = -10,
            IsMandatory = true,
            Severity = RuleSeverity.DaiHung,
            IsBlocked = true,
            Message = "Phạm Nguyệt Kỵ"
        };

        var eval2 = new RuleEvaluation
        {
            RuleCode = "SAT_CHU",
            IsPassed = false,
            ScoreImpact = -10,
            IsMandatory = true,
            Severity = RuleSeverity.DaiHung,
            IsBlocked = true,
            Message = "Phạm Sát Chủ"
        };

        // Act & Assert
        eval1.Should().NotBe(eval2);
    }

    [Fact]
    public void Inequality_DifferentIsPassed_AreNotEqual()
    {
        // Arrange
        var eval1 = new RuleEvaluation
        {
            RuleCode = "NGUYET_KY",
            IsPassed = false,
            ScoreImpact = -10,
            IsMandatory = true,
            Severity = RuleSeverity.DaiHung,
            IsBlocked = true,
            Message = "Phạm Nguyệt Kỵ"
        };

        var eval2 = new RuleEvaluation
        {
            RuleCode = "NGUYET_KY",
            IsPassed = true,
            ScoreImpact = 0,
            IsMandatory = false,
            Severity = RuleSeverity.None,
            IsBlocked = false,
            Message = "Không phạm Nguyệt Kỵ"
        };

        // Act & Assert
        eval1.Should().NotBe(eval2);
    }

    [Fact]
    public void Severity_None_HasValueZero()
    {
        // Arrange
        var evaluation = new RuleEvaluation
        {
            RuleCode = "TEST",
            IsPassed = true,
            ScoreImpact = 0,
            IsMandatory = false,
            Severity = RuleSeverity.None,
            IsBlocked = false,
            Message = "OK"
        };

        // Assert
        ((int)evaluation.Severity).Should().Be(0);
    }

    [Fact]
    public void Severity_Minor_HasValueOne()
    {
        // Arrange
        var evaluation = new RuleEvaluation
        {
            RuleCode = "TEST",
            IsPassed = false,
            ScoreImpact = -5,
            IsMandatory = false,
            Severity = RuleSeverity.Minor,
            IsBlocked = false,
            Message = "Minor issue"
        };

        // Assert
        ((int)evaluation.Severity).Should().Be(1);
    }

    [Fact]
    public void Severity_Moderate_HasValueTwo()
    {
        // Arrange
        var evaluation = new RuleEvaluation
        {
            RuleCode = "TEST",
            IsPassed = false,
            ScoreImpact = -8,
            IsMandatory = false,
            Severity = RuleSeverity.Moderate,
            IsBlocked = false,
            Message = "Moderate issue"
        };

        // Assert
        ((int)evaluation.Severity).Should().Be(2);
    }

    [Fact]
    public void Severity_Hung_HasValueThree()
    {
        // Arrange
        var evaluation = new RuleEvaluation
        {
            RuleCode = "TEST",
            IsPassed = false,
            ScoreImpact = -15,
            IsMandatory = true,
            Severity = RuleSeverity.Hung,
            IsBlocked = true,
            Message = "Hung"
        };

        // Assert
        ((int)evaluation.Severity).Should().Be(3);
    }

    [Fact]
    public void Severity_DaiHung_HasValueFour()
    {
        // Arrange
        var evaluation = new RuleEvaluation
        {
            RuleCode = "NGUYET_KY",
            IsPassed = false,
            ScoreImpact = -10,
            IsMandatory = true,
            Severity = RuleSeverity.DaiHung,
            IsBlocked = true,
            Message = "DaiHung"
        };

        // Assert
        ((int)evaluation.Severity).Should().Be(4);
    }

    [Fact]
    public void PassedResult_HasCorrectDefaults()
    {
        // Arrange & Act
        var evaluation = new RuleEvaluation
        {
            RuleCode = "TEST",
            IsPassed = true,
            ScoreImpact = 0,
            IsMandatory = false,
            Severity = RuleSeverity.None,
            IsBlocked = false,
            Message = "OK"
        };

        // Assert
        evaluation.IsPassed.Should().BeTrue();
        evaluation.ScoreImpact.Should().Be(0);
        evaluation.Severity.Should().Be(RuleSeverity.None);
        evaluation.IsBlocked.Should().BeFalse();
        evaluation.IsMandatory.Should().BeFalse();
    }

    [Fact]
    public void NegativeScoreImpact_IsValid()
    {
        // Arrange & Act
        var evaluation = new RuleEvaluation
        {
            RuleCode = "TEST",
            IsPassed = false,
            ScoreImpact = -100,
            IsMandatory = true,
            Severity = RuleSeverity.DaiHung,
            IsBlocked = true,
            Message = "Very bad"
        };

        // Assert
        evaluation.ScoreImpact.Should().Be(-100);
    }
}
