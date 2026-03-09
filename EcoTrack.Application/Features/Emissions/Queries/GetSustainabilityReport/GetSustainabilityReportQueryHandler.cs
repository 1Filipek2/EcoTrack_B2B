using EcoTrack.Application.DTOs;
using EcoTrack.Application.Interfaces;
using EcoTrack.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoTrack.Application.Features.Emissions.Queries.GetSustainabilityReport;

public class GetSustainabilityReportQueryHandler : IRequestHandler<GetSustainabilityReportQuery, SustainabilityReportDto>
{
    private readonly IEcoTrackDbContext _context;

    public GetSustainabilityReportQueryHandler(IEcoTrackDbContext context)
    {
        _context = context;
    }

    public async Task<SustainabilityReportDto> Handle(GetSustainabilityReportQuery request, CancellationToken cancellationToken)
    {
        var query = _context.EmissionEntries
            .Include(e => e.Company)
            .Include(e => e.Category)
            .Where(e => e.Company.Id == request.CompanyId);

        if (request.StartDate.HasValue)
            query = query.Where(e => e.ReportDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(e => e.ReportDate <= request.EndDate.Value);

        var emissions = await query.ToListAsync(cancellationToken);

        var scope1Total = emissions
            .Where(e => e.Category.Scope == EmissionScope.Scope1)
            .Sum(e => e.Co2Equivalent);

        var scope2Total = emissions
            .Where(e => e.Category.Scope == EmissionScope.Scope2)
            .Sum(e => e.Co2Equivalent);

        var scope3Total = emissions
            .Where(e => e.Category.Scope == EmissionScope.Scope3)
            .Sum(e => e.Co2Equivalent);

        return new SustainabilityReportDto
        {
            CompanyId = request.CompanyId,
            TotalEmissions = scope1Total + scope2Total + scope3Total,
            Scope1 = scope1Total,
            Scope2 = scope2Total,
            Scope3 = scope3Total
        };
    }
}

