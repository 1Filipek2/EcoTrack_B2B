using EcoTrack.Application.Interfaces;
using EcoTrack.Core.Entities;
using MediatR;

namespace EcoTrack.Application.Features.Emissions.Commands.CreateEmission;

public class CreateEmissionCommandHandler : IRequestHandler<CreateEmissionCommand, Guid>
{
    private readonly IEcoTrackDbContext _context;
    
    public CreateEmissionCommandHandler(IEcoTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(
        CreateEmissionCommand request,
        CancellationToken cancellationToken)
    {
        var company = await _context.Companies.FindAsync(request.CompanyId);
        var category = await _context.EmissionCategories.FindAsync(request.CategoryId);
        var entry = new EmissionEntry
        {
            Company = company!,
            Category = category!,
            Amount = request.Amount,
            ReportDate = request.ReportDate,
            RawData = request.RawData
        };
        
        _context.EmissionEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);
        return entry.Id;
    }
}