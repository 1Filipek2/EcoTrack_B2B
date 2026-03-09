using FluentValidation;

namespace EcoTrack.Application.Features.Emissions.Commands.CreateEmission;

public class CreateEmissionCommandValidator : AbstractValidator<CreateEmissionCommand>
{
    public CreateEmissionCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Company ID is required.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.RawData)
            .NotEmpty().WithMessage("Raw data is required.")
            .MaximumLength(2000).WithMessage("Raw data cannot exceed 2000 characters.");
    }
}

