using FluentValidation;

namespace ZenTam.Api.Features.ParseAndEvaluate;

public class ParseAndEvaluateValidator : AbstractValidator<ParseAndEvaluateRequest>
{
    public ParseAndEvaluateValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
    }
}
