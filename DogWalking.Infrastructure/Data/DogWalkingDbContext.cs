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
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Dog> Dogs => Set<Dog>();
    public DbSet<WalkEvent> WalkEvents => Set<WalkEvent>();
    public DbSet<WalkerAvailability> WalkerAvailabilities => Set<WalkerAvailability>();
    public DbSet<WalkerWorkingArea>  WalkerWorkingAreas   => Set<WalkerWorkingArea>();

    public DogWalkingDbContext(DbContextOptions<DogWalkingDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ClientConfiguration());
        modelBuilder.ApplyConfiguration(new DogConfiguration());
        modelBuilder.ApplyConfiguration(new WalkEventConfiguration());
        modelBuilder.ApplyConfiguration(new WalkerAvailabilityConfiguration());
        modelBuilder.ApplyConfiguration(new WalkerWorkingAreaConfiguration());
    }
}
