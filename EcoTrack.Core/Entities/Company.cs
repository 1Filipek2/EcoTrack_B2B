using EcoTrack.Core.Common;

namespace EcoTrack.Core.Entities;

public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string VatNumber { get; set; } = string.Empty;
    public List<EmissionEntry> EmissionEntries { get; set; } = new();

    public decimal GetTotalEmissions()
    {
        return EmissionEntries.Sum(e => e.Co2Equivalent);
    }
}