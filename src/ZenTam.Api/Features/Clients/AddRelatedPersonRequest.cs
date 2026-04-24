using FluentValidation;
using ZenTam.Api.Common.Domain;

namespace ZenTam.Api.Features.Clients;

public class AddRelatedPersonRequest
{
    public Guid ClientId { get; init; }
    public string Label { get; init; } = string.Empty;
    public DateTime SolarDob { get; init; }
    public Gender Gender { get; init; }
}

public class AddRelatedPersonValidator : AbstractValidator<AddRelatedPersonRequest>
{
    public AddRelatedPersonValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Label is required")
            .MaximumLength(50).WithMessage("Label must not exceed 50 characters");

        RuleFor(x => x.SolarDob)
            .NotEmpty().WithMessage("SolarDob is required")
            .Must(BeAPastDate).WithMessage("SolarDob must be in the past")
            .Must(BeValidAge).WithMessage("Age must be between 1 and 120 years");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Gender must be 0 (Male) or 1 (Female)");
    }

    private static bool BeAPastDate(DateTime date) => date < DateTime.UtcNow;

    private static bool BeValidAge(DateTime dob)
    {
        var age = DateTime.UtcNow.Year - dob.Year;
        if (DateTime.UtcNow < dob.AddYears(age)) age--;
        return age >= 1 && age <= 120;
    }
}
