using FluentValidation;
using ZenTam.Api.Common.Domain;

namespace ZenTam.Api.Features.Clients;

public class CreateClientRequest
{
    public string Name { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTime SolarDob { get; init; }
    public Gender? Gender { get; init; }
    public string? Notes { get; init; }
}

public class CreateClientValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("PhoneNumber is required")
            .MaximumLength(20).WithMessage("PhoneNumber must not exceed 20 characters")
            .Matches(@"^0\d{9,10}$").WithMessage("PhoneNumber must be a valid Vietnam phone number (09x or 01x format)");

        RuleFor(x => x.SolarDob)
            .NotEmpty().WithMessage("SolarDob is required")
            .Must(BeAPastDate).WithMessage("SolarDob must be in the past")
            .Must(BeValidAge).WithMessage("Age must be between 1 and 120 years");

        RuleFor(x => x.Gender)
            .NotNull().WithMessage("Gender is required")
            .IsInEnum().WithMessage("Gender must be 0 (Male) or 1 (Female)");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters")
            .When(x => x.Notes is not null);
    }

    private static bool BeAPastDate(DateTime date) => date < DateTime.UtcNow;

    private static bool BeValidAge(DateTime dob)
    {
        var age = DateTime.UtcNow.Year - dob.Year;
        if (DateTime.UtcNow < dob.AddYears(age)) age--;
        return age >= 1 && age <= 120;
    }
}
