using FluentValidation;

namespace EcoTrack.Application.Features.Emissions.Commands.ProcessUnstructuredData;

public class ProcessUnstructuredDataCommandValidator : AbstractValidator<ProcessUnstructuredDataCommand>
{
    public ProcessUnstructuredDataCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Company ID is required.");

        RuleFor(x => x.RawText)
            .NotEmpty().WithMessage("Raw text is required.")
            .MinimumLength(5).WithMessage("Raw text must be at least 5 characters.")
            .MaximumLength(5000).WithMessage("Raw text cannot exceed 5000 characters.");
    }
}

