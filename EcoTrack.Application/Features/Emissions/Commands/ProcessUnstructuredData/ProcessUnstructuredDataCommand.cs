using MediatR;

namespace EcoTrack.Application.Features.Emissions.Commands.ProcessUnstructuredData;

public record ProcessUnstructuredDataCommand(
    Guid CompanyId,
    string RawText,
    DateTimeOffset? ReportDate = null
) : IRequest<Guid>;

