using EcoTrack.Application.DTOs;
using MediatR;

namespace EcoTrack.Application.Features.Emissions.Queries.GetSustainabilityReport;

public record GetSustainabilityReportQuery(Guid CompanyId, DateTimeOffset? StartDate, DateTimeOffset? EndDate) 
    : IRequest<SustainabilityReportDto>;

