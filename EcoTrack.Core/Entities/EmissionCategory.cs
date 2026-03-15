using EcoTrack.Core.Common;
using EcoTrack.Core.Enums;

using Pgvector;

namespace EcoTrack.Core.Entities;

public class EmissionCategory : BaseEntity
{
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Pgvector.Vector? Embedding { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EmissionScope Scope { get; set; }
    public string? NameTranslations { get; set; }
}
