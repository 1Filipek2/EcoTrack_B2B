using EcoTrack.Application.DTOs;
namespace EcoTrack.Core.Applicattion.Interfaces;

public interface IAiExtractorService
{
    Task<EmissionDto> ExtractEmissionAsync(string rawText);
}