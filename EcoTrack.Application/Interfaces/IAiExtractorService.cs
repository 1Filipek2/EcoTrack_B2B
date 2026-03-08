using EcoTrack.Application.DTOs;
namespace EcoTrack.Application.Interfaces;

public interface IAiExtractorService
{
    Task<EmissionDto> ExtractEmissionAsync(string rawText);
}