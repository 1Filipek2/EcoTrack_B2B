using EcoTrack.Application.Common.Mapping;
using EcoTrack.Application.DTOs;
using EcoTrack.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoTrack.Application.Features.Emissions.Queries.GetCompanyEmissions;

public record GetCompanyEmissionQuery(Guid CompanyId) : IRequest<List<EmissionDto>>;

public class GetCompanyEmissionsQueryHandler : IRequestHandler<GetCompanyEmissionQuery, List<EmissionDto>>
{
    private readonly IEcoTrackDbContext _context;

    public GetCompanyEmissionsQueryHandler(IEcoTrackDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmissionDto>> Handle(GetCompanyEmissionQuery request, CancellationToken cancellationToken)
    {
        var emissions = await _context.EmissionEntries
            .Include(e => e.Company)
            .Include(e => e.Category)
            .Where(e => e.Company.Id == request.CompanyId)
            .ToListAsync(cancellationToken);

        return emissions.Select(e => e.ToDto()).ToList();
    }
}