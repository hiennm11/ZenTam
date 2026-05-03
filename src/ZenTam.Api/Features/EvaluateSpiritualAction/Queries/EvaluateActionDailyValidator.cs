using FluentValidation;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Queries;

/// <summary>
/// Validator for EvaluateActionDailyRequest.
/// </summary>
public class EvaluateActionDailyValidator : AbstractValidator<EvaluateActionDailyRequest>
{
    public EvaluateActionDailyValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.ActionCode)
            .NotEmpty().WithMessage("ActionCode is required")
            .MaximumLength(50).WithMessage("ActionCode cannot exceed 50 characters");

        RuleFor(x => x.TargetDate)
            .NotEmpty().WithMessage("TargetDate is required")
            .Must(d => d >= DateOnly.FromDateTime(DateTime.Today.AddYears(-10)))
            .WithMessage("TargetDate must be within 10 years past or future");
    }
}