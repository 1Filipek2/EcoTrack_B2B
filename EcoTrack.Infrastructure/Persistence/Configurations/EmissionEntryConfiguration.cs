using EcoTrack.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace EcoTrack.Infrastructure.Persistence.Configurations;
public class EmissionEntryConfiguration : IEntityTypeConfiguration<EmissionEntry>
{
    public void Configure(EntityTypeBuilder<EmissionEntry> builder)
    {
        builder.ToTable("EmissionEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReportDate)
            .IsRequired();
        
        builder.Property(e => e.RawData)
            .HasMaxLength(2000);
        
        builder.Property<Vector>("Embedding")
            .HasColumnType("vector(1536)"); 
        
        builder.HasOne(e => e.Company)
            .WithMany(c => c.EmissionEntries)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Category)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ReportDate);
    }
}