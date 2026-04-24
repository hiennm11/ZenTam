using FluentValidation;

namespace ZenTam.Api.Features.ParseAndEvaluate;

public class ParseAndEvaluateValidator : AbstractValidator<ParseAndEvaluateRequest>
{
    public ParseAndEvaluateValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty().WithMessage("ClientId is required");
        RuleFor(x => x.Message).NotEmpty().MaximumLength(1000);
    }
}
