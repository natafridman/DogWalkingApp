using DogWalking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DogWalking.Infrastructure;

public class DbContextFactory : IDesignTimeDbContextFactory<DogWalkingDbContext>
{
    public DogWalkingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DogWalkingDbContext>();

        // TODO: Move the connection string to a configuration file or environment variable
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=DogWalkingApp;Trusted_Connection=True;TrustServerCertificate=True");

        return new DogWalkingDbContext(optionsBuilder.Options);
    }
}