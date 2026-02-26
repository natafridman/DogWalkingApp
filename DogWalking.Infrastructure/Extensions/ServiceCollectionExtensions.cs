using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Application.Services;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Interfaces;
using DogWalking.Infrastructure;
using DogWalking.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DogWalking.Infrastructure.Extensions;

/// <summary>
/// Extension method that registers all infrastructure and application services.
/// Keeps Program.cs clean — all wiring in one place.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDogWalkingServices(this IServiceCollection services, string conn)
    {
        services.AddDbContext<DogWalkingDbContext>(o => o.UseSqlServer(conn));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }

    /// <summary>
    /// Applies pending EF Core migrations and seeds default users on first run.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<DogWalkingDbContext>();
        await ctx.Database.MigrateAsync();

        var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();

        // Seed default admin
        await auth.CreateUserAsync(new CreateUserDto(
            "admin", "admin123", "System Administrator", UserRole.Admin));

        // Seed a default client for demo
        await auth.CreateUserAsync(new CreateUserDto(
            "nata", "natafridman", "Nataniel Fridman", UserRole.Client));

        // Seed a default walker for demo
        await auth.CreateUserAsync(new CreateUserDto(
            "nataw", "natafridman", "Nataniel Fridman Walker", UserRole.Walker));
    }
}
