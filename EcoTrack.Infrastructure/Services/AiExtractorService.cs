using EcoTrack.Application.DTOs;
using EcoTrack.Application.Interfaces;

namespace EcoTrack.Infrastructure.Services;
public class AiExtractorService : IAiExtractorService
{
    public Task<EmissionDto> ExtractEmissionAsync(string rawText)
    {
        return Task.FromResult(new EmissionDto
        {
            Id = Guid.NewGuid(),
            CompanyId = Guid.Empty,
            Category = "Scope1",
            Amount = 100,
            Co2Equivalent = 233,
            ReportDate = DateTimeOffset.UtcNow
        });
    }
}