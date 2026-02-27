using DogWalking.Application.DTOs;
using DogWalking.Domain.Enums;

namespace DogWalking.IntegrationTests.Services;

public class WalkerAvailabilityServiceTests : IntegrationTestBase
{
    [Fact]
    public async Task AddAvailabilityAsync_Valid_PersistsSlot()
    {
        var walker = await SeedUserAsync("walker1", UserRole.Walker, "Walk Person");
        var svc = CreateAvailabilityService();

        var result = await svc.AddAvailabilityAsync(
            new CreateAvailabilityDto(walker.Id, DayOfWeek.Monday,
                new TimeOnly(8, 0), new TimeOnly(12, 0), "Palermo"));

        Assert.True(result.Id > 0);
        Assert.Equal(DayOfWeek.Monday, result.DayOfWeek);
        Assert.Equal("Palermo", result.Zone);
    }

    [Fact]
    public async Task DeleteAvailabilityAsync_RemovesSlot()
    {
        var walker = await SeedUserAsync("walker1", UserRole.Walker, "Walk Person");
        var slot = await SeedAvailabilityAsync(walker.Id, DayOfWeek.Tuesday,
            new TimeOnly(9, 0), new TimeOnly(14, 0), "Recoleta");
        var svc = CreateAvailabilityService();

        await svc.DeleteAvailabilityAsync(slot.Id);

        var remaining = await Uow.WalkerAvailabilities.GetByWalkerIdAsync(walker.Id);
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task GetByWalkerIdAsync_ReturnsOnlyWalkerSlots()
    {
        var walker1 = await SeedUserAsync("walker1", UserRole.Walker, "Walker One");
        var walker2 = await SeedUserAsync("walker2", UserRole.Walker, "Walker Two");
        await SeedAvailabilityAsync(walker1.Id, DayOfWeek.Monday);
        await SeedAvailabilityAsync(walker1.Id, DayOfWeek.Wednesday);
        await SeedAvailabilityAsync(walker2.Id, DayOfWeek.Friday);
        var svc = CreateAvailabilityService();

        var result = (await svc.GetByWalkerIdAsync(walker1.Id)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, s => Assert.Equal(walker1.Id, s.WalkerId));
    }

    [Fact]
    public async Task FindEligibleWalkersAsync_MatchingSlot_ReturnsWalker()
    {
        var walker = await SeedUserAsync("walker1", UserRole.Walker, "Walk Person");
        var client = await SeedClientAsync(subscription: SubscriptionType.Basic);
        var dog = await SeedDogAsync(client.Id);

        // Build walk date in LOCAL time so ToLocalTime() roundtrips correctly
        var localMonday = DateTime.Now.Date;
        while (localMonday.DayOfWeek != DayOfWeek.Monday)
            localMonday = localMonday.AddDays(1);
        var localWalkTime = localMonday.AddHours(10); // Monday 10:00 local
        var walkDateUtc = localWalkTime.ToUniversalTime();

        var walk = await SeedWalkEventAsync(dog.Id, walkDateUtc, 60, "Palermo");

        // Availability covers Monday 08:00-18:00 local, zone "Palermo"
        await SeedAvailabilityAsync(walker.Id, DayOfWeek.Monday,
            new TimeOnly(8, 0), new TimeOnly(18, 0), "Palermo");

        var svc = CreateAvailabilityService();
        var result = (await svc.FindEligibleWalkersAsync(walk.Id)).ToList();

        Assert.Single(result);
        Assert.Equal(walker.Id, result[0].WalkerId);
    }

    [Fact]
    public async Task FindEligibleWalkersAsync_WrongZone_ReturnsEmpty()
    {
        var walker = await SeedUserAsync("walker1", UserRole.Walker, "Walk Person");
        var client = await SeedClientAsync(subscription: SubscriptionType.Basic);
        var dog = await SeedDogAsync(client.Id);

        var localMonday = DateTime.Now.Date;
        while (localMonday.DayOfWeek != DayOfWeek.Monday)
            localMonday = localMonday.AddDays(1);
        var walkDateUtc = localMonday.AddHours(10).ToUniversalTime();

        var walk = await SeedWalkEventAsync(dog.Id, walkDateUtc, 60, "Palermo");

        // Walker's zone is "Belgrano" — does not match
        await SeedAvailabilityAsync(walker.Id, DayOfWeek.Monday,
            new TimeOnly(8, 0), new TimeOnly(18, 0), "Belgrano");

        var svc = CreateAvailabilityService();
        var result = (await svc.FindEligibleWalkersAsync(walk.Id)).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public async Task FindEligibleWalkersAsync_ConflictingWalk_ExcludesWalker()
    {
        var walker = await SeedUserAsync("walker1", UserRole.Walker, "Walk Person");
        var client = await SeedClientAsync(subscription: SubscriptionType.Premium);
        var dog1 = await SeedDogAsync(client.Id, "Dog1");
        var dog2 = await SeedDogAsync(client.Id, "Dog2");

        var localMonday = DateTime.Now.Date;
        while (localMonday.DayOfWeek != DayOfWeek.Monday)
            localMonday = localMonday.AddDays(1);
        var walkDateUtc = localMonday.AddHours(10).ToUniversalTime();

        // Walker has availability
        await SeedAvailabilityAsync(walker.Id, DayOfWeek.Monday,
            new TimeOnly(8, 0), new TimeOnly(18, 0), "Palermo");

        // Walker already has a conflicting walk (proposed) at the same time
        var existingWalk = await SeedWalkEventAsync(dog1.Id, walkDateUtc, 60, "Palermo");
        existingWalk.ProposeToWalker(walker.Id);
        Uow.WalkEvents.Update(existingWalk);
        await Uow.CommitAsync();

        // New walk request at same time
        var newWalk = await SeedWalkEventAsync(dog2.Id, walkDateUtc, 60, "Palermo");

        var svc = CreateAvailabilityService();
        var result = (await svc.FindEligibleWalkersAsync(newWalk.Id)).ToList();

        Assert.Empty(result);
    }
}
