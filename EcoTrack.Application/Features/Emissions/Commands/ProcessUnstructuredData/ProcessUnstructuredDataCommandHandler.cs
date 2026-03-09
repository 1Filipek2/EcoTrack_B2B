using EcoTrack.Application.Interfaces;
using EcoTrack.Core.Common;
using EcoTrack.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoTrack.Application.Features.Emissions.Commands.ProcessUnstructuredData;

public class ProcessUnstructuredDataCommandHandler : IRequestHandler<ProcessUnstructuredDataCommand, Guid>
{
    private readonly IEcoTrackDbContext _context;
    private readonly IAiExtractorService _aiExtractorService;

    public ProcessUnstructuredDataCommandHandler(IEcoTrackDbContext context, IAiExtractorService aiExtractorService)
    {
        _context = context;
        _aiExtractorService = aiExtractorService;
    }

    public async Task<Guid> Handle(ProcessUnstructuredDataCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException($"Company with id '{request.CompanyId}' was not found.");

        var extracted = await _aiExtractorService.ExtractAndMapAsync(request.RawText, cancellationToken);

        EmissionCategory? category = null;
        if (extracted.MatchedCategoryId.HasValue)
        {
            category = await _context.EmissionCategories
                .FirstOrDefaultAsync(c => c.Id == extracted.MatchedCategoryId.Value, cancellationToken);
        }

        category ??= await _context.EmissionCategories
            .FirstOrDefaultAsync(
                c => c.Name.ToLower().Contains(extracted.Category.ToLower()),
                cancellationToken);

        category ??= await _context.EmissionCategories.FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No emission categories are available in the database.");

        var emissionEntry = new EmissionEntry
        {
            Company = company,
            Category = category,
            Amount = extracted.Amount,
            RawData = request.RawText,
            ReportDate = request.ReportDate ?? DateTimeOffset.UtcNow
        };

        emissionEntry.SetEmissionFactor(ResolveEmissionFactor(category.Name, extracted.Unit));

        _context.EmissionEntries.Add(emissionEntry);
        await _context.SaveChangesAsync(cancellationToken);

        return emissionEntry.Id;
    }

    private static decimal ResolveEmissionFactor(string categoryName, string unit)
    {
        var normalizedCategory = categoryName.ToLowerInvariant();
        var normalizedUnit = unit.ToLowerInvariant();

        if (normalizedUnit.Contains("kwh") || normalizedCategory.Contains("electric"))
            return Constants.ElectricityEmissionFactor;

        if (normalizedUnit.Contains("m3") || normalizedCategory.Contains("gas"))
            return Constants.NaturalGasEmissionFactor;

        if (normalizedUnit.Contains("km") || normalizedCategory.Contains("transport") || normalizedCategory.Contains("trip"))
            return Constants.TransportEmissionFactor;

        return Constants.TransportEmissionFactor;
    }
}
