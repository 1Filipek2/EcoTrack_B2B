using EcoTrack.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;

namespace EcoTrack.Infrastructure.Persistence.Configurations;

public class EmissionCategoryConfiguration : IEntityTypeConfiguration<EmissionCategory>
{
    public void Configure(EntityTypeBuilder<EmissionCategory> builder)
    {
        builder.ToTable("EmissionCategories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.Scope)
            .IsRequired();

        builder.Property<Vector>("Embedding")
            .HasColumnType("vector(1536)");
    }
}