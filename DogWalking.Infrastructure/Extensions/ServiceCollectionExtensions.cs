using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Application.Services;
using DogWalking.Application.Validators;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Interfaces;
using DogWalking.Infrastructure.Data;
using DogWalking.Infrastructure.Messaging;

using FluentValidation;
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
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IDogService, DogService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IWalkEventService, WalkEventService>();
        services.AddScoped<IWalkerAvailabilityService, WalkerAvailabilityService>();

        services.AddValidatorsFromAssemblyContaining<CreateClientDtoValidator>();

        // LAN notification service (singleton — one listener for the app lifetime)
        services.AddSingleton<INotificationService, UdpNotificationService>();

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
        await auth.RegisterClientUserAsync(new RegisterClientUserDto(
            "nata", "natafridman", "Nataniel Fridman",
            "+1234567890", "nata@test.com", SubscriptionType.Basic, "123 Demo Street", "natafridman"));

        // Seed a default walker for demo
        await auth.CreateUserAsync(new CreateUserDto(
            "nataw", "natafridman", "Nataniel Fridman Walker", UserRole.Walker));
    }
}
