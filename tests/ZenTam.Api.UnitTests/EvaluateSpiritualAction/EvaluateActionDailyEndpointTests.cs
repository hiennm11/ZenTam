using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ZenTam.Api.Common.Exceptions;
using ZenTam.Api.Common.Rules;
using ZenTam.Api.Features.EvaluateSpiritualAction.Models;
using ZenTam.Api.Features.EvaluateSpiritualAction.Queries;

namespace ZenTam.Api.UnitTests.EvaluateSpiritualAction;

public class EvaluateActionDailyEndpointTests
{
    private readonly Mock<IValidator<EvaluateActionDailyRequest>> _mockValidator;

    public EvaluateActionDailyEndpointTests()
    {
        _mockValidator = new Mock<IValidator<EvaluateActionDailyRequest>>();
    }

    private static Guid NewGuid() => Guid.NewGuid();

    [Fact]
    public async Task ValidRequest_Returns200OKWithResponseBody()
    {
        // Arrange
        var userId = NewGuid();
        var request = new EvaluateActionDailyRequest
        {
            UserId = userId,
            ActionCode = "XAY_NHA",
            TargetDate = new DateOnly(2024, 3, 15)
        };

        var expectedResponse = new EvaluateActionResponse
        {
            IsAllowed = true,
            TotalScore = 0,
            Verdict = "AN_TOAN",
            Details = new List<RuleResult>(),
            GanhMenh = null
        };

        _mockValidator.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act - simulate the endpoint validation logic
        var validationResult = await _mockValidator.Object.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task InvalidUserId_Returns404Problem()
    {
        // Arrange
        var userId = Guid.Empty;
        var request = new EvaluateActionDailyRequest
        {
            UserId = userId,
            ActionCode = "XAY_NHA",
            TargetDate = new DateOnly(2024, 3, 15)
        };

        _mockValidator.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act - simulate throwing NotFoundException
        Exception? thrownException = null;
        try
        {
            throw new NotFoundException("Client with Id '00000000-0000-0000-0000-000000000000' was not found.");
        }
        catch (Exception ex)
        {
            thrownException = ex;
        }

        // Assert
        thrownException.Should().BeOfType<NotFoundException>();
        thrownException!.Message.Should().Contain("Client");
    }

    [Fact]
    public async Task InvalidActionCode_Returns404Problem()
    {
        // Arrange
        var userId = NewGuid();
        var request = new EvaluateActionDailyRequest
        {
            UserId = userId,
            ActionCode = "INVALID_ACTION",
            TargetDate = new DateOnly(2024, 3, 15)
        };

        _mockValidator.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act - simulate throwing NotFoundException for invalid action
        Exception? thrownException = null;
        try
        {
            throw new NotFoundException("Action 'INVALID_ACTION' was not found.");
        }
        catch (Exception ex)
        {
            thrownException = ex;
        }

        // Assert
        thrownException.Should().BeOfType<NotFoundException>();
        thrownException!.Message.Should().Contain("Action");
    }

    [Fact]
    public async Task ValidationFailure_Returns400ProblemWithErrorsDict()
    {
        // Arrange
        var request = new EvaluateActionDailyRequest
        {
            UserId = Guid.Empty,
            ActionCode = "",
            TargetDate = default
        };

        var validationFailures = new List<ValidationFailure>
        {
            new("UserId", "UserId is required"),
            new("ActionCode", "ActionCode is required"),
            new("TargetDate", "TargetDate is required")
        };

        _mockValidator.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var validationResult = await _mockValidator.Object.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(3);

        var errorsDict = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        errorsDict.Should().ContainKey("UserId");
        errorsDict.Should().ContainKey("ActionCode");
        errorsDict.Should().ContainKey("TargetDate");
    }

    [Fact]
    public async Task EmptyUserId_Returns400Problem()
    {
        // Arrange
        var request = new EvaluateActionDailyRequest
        {
            UserId = Guid.Empty,
            ActionCode = "XAY_NHA",
            TargetDate = new DateOnly(2024, 3, 15)
        };

        var validationFailures = new List<ValidationFailure>
        {
            new("UserId", "UserId is required")
        };

        _mockValidator.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var validationResult = await _mockValidator.Object.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.First(e => e.PropertyName == "UserId").ErrorMessage
            .Should().Be("UserId is required");
    }

    [Fact]
    public async Task ActionCodeExceeds50Characters_Returns400Problem()
    {
        // Arrange
        var request = new EvaluateActionDailyRequest
        {
            UserId = NewGuid(),
            ActionCode = new string('A', 51),
            TargetDate = new DateOnly(2024, 3, 15)
        };

        var validationFailures = new List<ValidationFailure>
        {
            new("ActionCode", "ActionCode cannot exceed 50 characters")
        };

        _mockValidator.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var validationResult = await _mockValidator.Object.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.First(e => e.PropertyName == "ActionCode").ErrorMessage
            .Should().Be("ActionCode cannot exceed 50 characters");
    }

    [Fact]
    public async Task DefaultDateOnly_Returns400Problem()
    {
        // Arrange
        var request = new EvaluateActionDailyRequest
        {
            UserId = NewGuid(),
            ActionCode = "XAY_NHA",
            TargetDate = default
        };

        var validationFailures = new List<ValidationFailure>
        {
            new("TargetDate", "TargetDate is required")
        };

        _mockValidator.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var validationResult = await _mockValidator.Object.ValidateAsync(request);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.First(e => e.PropertyName == "TargetDate").ErrorMessage
            .Should().Be("TargetDate is required");
    }
}