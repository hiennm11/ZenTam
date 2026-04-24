using FluentValidation;

namespace ZenTam.Api.Features.ParseAndEvaluate;

public class ParseAndEvaluateValidator : AbstractValidator<ParseAndEvaluateRequest>
{
    public ParseAndEvaluateValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("ClientId is required")
            .NotEqual(Guid.Empty).WithMessage("ClientId cannot be empty");
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(500).WithMessage("Message must not exceed 500 characters");
    }
}
