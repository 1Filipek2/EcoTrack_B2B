using EcoTrack.Core.Common;

namespace EcoTrack.Core.Entities;

public class EmissionEntry : BaseEntity
{
    public decimal _emissionFactor;
    public decimal Amount { get; set { field = value; Co2Equivalent = field * _emissionFactor; }}
    public Company Company { get; set; } = null!;
    public EmissionCategory Category { get; set; } = null!;
    public DateTimeOffset ReportDate { get; set; }
    public string RawData { get; set; } = string.Empty;
    
    public decimal Co2Equivalent { get; private set; }

    public void SetEmissionFactor(decimal factor)
    {
        _emissionFactor = factor;
        Co2Equivalent = Amount * _emissionFactor;
    }
}