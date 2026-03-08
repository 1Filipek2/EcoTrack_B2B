using EcoTrack.Application.DTOs;
using EcoTrack.Core.Entities;

namespace EcoTrack.Application.Common.Mapping;

public static class EmissionMappings
{
    public static EmissionDto ToDto(this EmissionEntry entity)
    {
        return new EmissionDto
        {
            Id = entity.Id,
            CompanyId = entity.Company.Id,
            Category = entity.Category.Name,
            Amount = entity.Amount,
            Co2Equivalent = entity.Co2Equivalent,
            ReportDate = entity.ReportDate
        };
    }
}