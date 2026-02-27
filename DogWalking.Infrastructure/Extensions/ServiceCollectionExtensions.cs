using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Application.Services;
using DogWalking.Application.Validators;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Interfaces;
using DogWalking.Infrastructure.AI;
using DogWalking.Infrastructure.Data;
using DogWalking.Infrastructure.Messaging;

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DogWalking.Infrastructure.Extensions;

/// <summary>
/// Focused extension methods that each register a single concern.
/// Each method reads its own configuration section — callers just pass IConfiguration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers EF Core DbContext and UnitOfWork.
    /// Reads <c>ConnectionStrings:Default</c>.
    /// </summary>
    public static IServiceCollection AddDatabase(
        this IServiceCollection services, IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Missing 'Default' connection string in configuration.");

        services.AddDbContext<DogWalkingDbContext>(o => o.UseSqlServer(conn));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Registers all scoped application services and FluentValidation validators.
    /// </summary>
    public static IServiceCollection AddServices(
        this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IDogService, DogService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IWalkEventService, WalkEventService>();
        services.AddScoped<IWalkerAvailabilityService, WalkerAvailabilityService>();

        services.AddValidatorsFromAssemblyContaining<CreateClientDtoValidator>();

        return services;
    }

    /// <summary>
    /// Registers in-memory caching.
    /// </summary>
    public static IServiceCollection AddCaching(
        this IServiceCollection services)
    {
        services.AddMemoryCache();
        return services;
    }

    /// <summary>
    /// Registers the LAN notification service (singleton — one listener for the app lifetime).
    /// </summary>
    public static IServiceCollection AddNotifications(
        this IServiceCollection services)
    {
        services.AddSingleton<INotificationService, UdpNotificationService>();
        return services;
    }

    /// <summary>
    /// Registers the AI walk parser service (singleton — shares HttpClient and API key).
    /// Reads <c>OpenAI:ApiKey</c>.
    /// </summary>
    public static IServiceCollection AddAI(
        this IServiceCollection services, IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"] ?? "";

        services.AddSingleton<IAiWalkParserService>(
            _ => new OpenAiWalkParserService(new HttpClient(), apiKey));

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
