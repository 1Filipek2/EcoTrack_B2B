using EcoTrack.Core.Common;

using Pgvector;

namespace EcoTrack.Core.Entities;

public class EmissionEntry : BaseEntity
{
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Pgvector.Vector? Embedding { get; set; }
    public decimal Amount { get; set; }
    public Company Company { get; set; } = null!;
    public EmissionCategory Category { get; set; } = null!;
    public DateTimeOffset ReportDate { get; set; }
    public string RawData { get; set; } = string.Empty;
    public decimal Co2Equivalent { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}