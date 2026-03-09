using EcoTrack.Application.Interfaces;
using EcoTrack.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace EcoTrack.Infrastructure.Persistence;
public class EcoTrackDbContext : DbContext, IEcoTrackDbContext
{
    public EcoTrackDbContext(DbContextOptions<EcoTrackDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<EmissionCategory> EmissionCategories => Set<EmissionCategory>();
    public DbSet<EmissionEntry> EmissionEntries => Set<EmissionEntry>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EcoTrackDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }


}