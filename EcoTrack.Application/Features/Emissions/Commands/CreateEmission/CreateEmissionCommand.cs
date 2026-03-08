using MediatR;

namespace EcoTrack.Application.Features.Emissions.Commands.CreateEmission;

public record CreateEmissionCommand (
    Guid CompanyId,
    Guid CategoryId,
    decimal Amount,
    DateTimeOffset ReportDate,
    string RawData
) : IRequest<Guid>;