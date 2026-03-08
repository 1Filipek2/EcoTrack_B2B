using EcoTrack.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoTrack.Infrastructure.Persistence.Configurations;
public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200); 
        
        builder.Property(c => c.VatNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasMany(c => c.EmissionEntries)
            .WithOne(e => e.Company)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.VatNumber)
            .IsUnique();
    }
}