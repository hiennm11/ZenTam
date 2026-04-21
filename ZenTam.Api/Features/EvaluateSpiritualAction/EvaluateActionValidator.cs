using FluentValidation;

namespace ZenTam.Api.Features.EvaluateSpiritualAction;

public class EvaluateActionValidator : AbstractValidator<EvaluateActionRequest>
{
    public EvaluateActionValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.ActionCode)
            .NotEmpty().WithMessage("ActionCode is required.")
            .MaximumLength(50);

        RuleFor(x => x.TargetYear)
            .Must(y => y >= DateTime.Now.Year)
            .WithMessage(_ => $"TargetYear must be >= {DateTime.Now.Year}.")
            .Must(y => y <= 2100)
            .WithMessage("TargetYear must be <= 2100.");
    }
}
