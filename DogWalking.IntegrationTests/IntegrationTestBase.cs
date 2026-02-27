using System.Security.Cryptography;
using System.Text;
using DogWalking.Application.Services;
using DogWalking.Application.Validators;
using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;
using DogWalking.Infrastructure;
using DogWalking.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace DogWalking.IntegrationTests;

/// <summary>
/// Base class for integration tests.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected DogWalkingDbContext Ctx { get; }
    protected UnitOfWork Uow { get; }
    private readonly SqliteConnection _conn;

    protected IntegrationTestBase()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var options = new DbContextOptionsBuilder<DogWalkingDbContext>()
            .UseSqlite(_conn).Options;
        Ctx = new DogWalkingDbContext(options);
        Ctx.Database.EnsureCreated();
        Uow = new UnitOfWork(Ctx, new MemoryCache(new MemoryCacheOptions()));
    }

    // ── Service factories ──────────────────────────────────────────────────

    protected AuthService CreateAuthService() => new(
        Uow,
        new LoginDtoValidator(),
        new CreateUserDtoValidator(),
        new RegisterClientUserDtoValidator()
    );

    protected ClientService CreateClientService() => new(
        Uow,
        NullLogger<ClientService>.Instance,
        new CreateClientDtoValidator()
    );

    protected DogService CreateDogService() => new(Uow);

    protected WalkEventService CreateWalkEventService() => new(
        Uow,
        NullLogger<WalkEventService>.Instance,
        new CreateWalkEventDtoValidator()
    );

    protected WalkerAvailabilityService CreateAvailabilityService() => new(Uow);

    protected UserService CreateUserService() => new(Uow);

    // ── Seed helpers ───────────────────────────────────────────────────────

    protected async Task<User> SeedUserAsync(
        string username = "testuser",
        UserRole role = UserRole.Admin,
        string fullName = "Test User",
        string password = "Pass123!",
        string? phone = null,
        string? email = null)
    {
        var hash = HashPassword(password);
        var user = new User(username, hash, fullName, role, phone, email);
        await Uow.Users.AddAsync(user);
        await Uow.CommitAsync();
        return user;
    }

    protected async Task<Client> SeedClientAsync(
        string name = "Test Client",
        string phone = "+1234567890",
        string email = "client@test.com",
        SubscriptionType subscription = SubscriptionType.Free,
        string zone = "TestZone",
        string address = "123 Test St")
    {
        var client = new Client(name, phone, email, subscription, zone: zone, address: address);
        await Uow.Clients.AddAsync(client);
        await Uow.CommitAsync();
        return client;
    }

    protected async Task<Dog> SeedDogAsync(
        int clientId,
        string name = "Buddy",
        string breed = "Labrador",
        DateOnly? birthDate = null)
    {
        var bd = birthDate ?? DateOnly.FromDateTime(DateTime.Today.AddYears(-3));
        var dog = new Dog(clientId, name, breed, bd);
        await Uow.Dogs.AddAsync(dog);
        await Uow.CommitAsync();
        return dog;
    }

    protected async Task<WalkEvent> SeedWalkEventAsync(
        int dogId,
        DateTime? walkDate = null,
        int durationMinutes = 60,
        string location = "TestZone",
        string? notes = null)
    {
        var date = walkDate ?? DateTime.UtcNow.AddDays(1);
        var walk = new WalkEvent(dogId, date, durationMinutes, location, notes);
        await Uow.WalkEvents.AddAsync(walk);
        await Uow.CommitAsync();
        return walk;
    }

    protected async Task<WalkerAvailability> SeedAvailabilityAsync(
        int walkerId,
        DayOfWeek dayOfWeek = DayOfWeek.Monday,
        TimeOnly? start = null,
        TimeOnly? end = null,
        string zone = "TestZone")
    {
        var s = start ?? new TimeOnly(8, 0);
        var e = end ?? new TimeOnly(18, 0);
        var slot = new WalkerAvailability(walkerId, dayOfWeek, s, e, zone);
        await Uow.WalkerAvailabilities.AddAsync(slot);
        await Uow.CommitAsync();
        return slot;
    }

    protected static string HashPassword(string password) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

    public void Dispose()
    {
        Ctx.Dispose();
        _conn.Dispose();
        GC.SuppressFinalize(this);
    }
}
