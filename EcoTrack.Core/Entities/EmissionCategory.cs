using EcoTrack.Core.Common;
using EcoTrack.Core.Enums;

namespace EcoTrack.Core.Entities;

public class EmissionCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EmissionScope Scope { get; set; }
}
