using DogWalking.Domain.Entities;
using DogWalking.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DogWalking.Infrastructure.Data;

/// <summary>
/// EF Core DbContext.
/// Entity mappings are kept in separate IEntityTypeConfiguration classes (Single Responsibility).
/// </summary>
public class DogWalkingDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public DogWalkingDbContext(DbContextOptions<DogWalkingDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
