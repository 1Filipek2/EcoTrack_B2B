using EcoTrack.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoTrack.Application.Interfaces;

public interface IEcoTrackDbContext
{
    DbSet<Company> Companies { get;}
    DbSet<EmissionEntry> EmissionEntries { get;}
    DbSet<EmissionCategory> EmissionCategories { get; }
    DbSet<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}