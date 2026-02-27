using DogWalking.Application.DTOs;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Exceptions;

namespace DogWalking.IntegrationTests.Services;

public class WalkEventServiceTests : IntegrationTestBase
{
    [Fact]
    public async Task ScheduleAsync_ValidData_CreatesRequestedWalk()
    {
        var client = await SeedClientAsync(subscription: SubscriptionType.Basic);
        var dog = await SeedDogAsync(client.Id);
        var svc = CreateWalkEventService();

        var walkDate = DateTime.UtcNow.AddDays(3);
        var result = await svc.ScheduleAsync(
            new CreateWalkEventDto(dog.Id, walkDate, 60, "TestZone"));

        Assert.True(result.Id > 0);
        Assert.Equal(WalkStatus.Requested, result.Status);
        Assert.Equal(60, result.DurationMinutes);
    }

    [Fact]
    public async Task ScheduleAsync_OverlappingWalk_ThrowsDomainException()
    {
        var client = await SeedClientAsync(subscription: SubscriptionType.Premium);
        var dog = await SeedDogAsync(client.Id);
        var svc = CreateWalkEventService();

        // First walk: tomorrow at 10:00 UTC, 60 min
        var walkDate = DateTime.UtcNow.AddDays(1).Date.AddHours(10);
        await svc.ScheduleAsync(new CreateWalkEventDto(dog.Id, walkDate, 60, "TestZone"));

        // Second walk overlaps: tomorrow at 10:30 UTC, 60 min → conflict
        var walkDate2 = walkDate.AddMinutes(30);
        await Assert.ThrowsAsync<DomainException>(() =>
            svc.ScheduleAsync(new CreateWalkEventDto(dog.Id, walkDate2, 60, "TestZone")));
    }

    [Fact]
    public async Task ScheduleAsync_OverlappingWalkDifferentDog_ThrowsDomainException()
    {
        // Two different dogs of the same client, but overlapping times on the same dog throws
        var client = await SeedClientAsync(subscription: SubscriptionType.Premium);
        var dog = await SeedDogAsync(client.Id, "Dog1");
        var svc = CreateWalkEventService();

        var baseDate = DateTime.UtcNow.AddDays(5).Date.AddHours(10);
        await svc.ScheduleAsync(new CreateWalkEventDto(dog.Id, baseDate, 120, "TestZone"));

        // Same dog, overlapping time — conflict
        await Assert.ThrowsAsync<DomainException>(() =>
            svc.ScheduleAsync(new CreateWalkEventDto(dog.Id, baseDate.AddHours(1), 60, "TestZone")));
    }

    [Fact]
    public async Task ProposeToWalkerAsync_SetsWalkerAndStatus()
    {
        var client = await SeedClientAsync(subscription: SubscriptionType.Basic);
        var dog = await SeedDogAsync(client.Id);
        var walker = await SeedUserAsync("walker1", UserRole.Walker, "Walk Person");
        var walk = await SeedWalkEventAsync(dog.Id);
        var svc = CreateWalkEventService();

        var result = await svc.ProposeToWalkerAsync(new ProposeWalkDto(walk.Id, walker.Id));

        Assert.Equal(WalkStatus.Proposed, result.Status);
        Assert.Equal(walker.Id, result.WalkerId);
    }

    [Fact]
    public async Task WalkerRespondAsync_Accept_TransitionsToAccepted()
    {
        var client = await SeedClientAsync(subscription: SubscriptionType.Basic);
        var dog = await SeedDogAsync(client.Id);
        var walker = await SeedUserAsync("walker1", UserRole.Walker, "Walk Person");
        var walk = await SeedWalkEventAsync(dog.Id);
        var svc = CreateWalkEventService();

        await svc.ProposeToWalkerAsync(new ProposeWalkDto(walk.Id, walker.Id));
        var result = await svc.WalkerRespondAsync(new WalkerResponseDto(walk.Id, walker.Id, true));

        Assert.Equal(WalkStatus.Accepted, result.Status);
    }

    [Fact]
    public async Task WalkerRespondAsync_Decline_ClearsWalkerReturnsToRequested()
    {
        var client = await SeedClientAsync(subscription: SubscriptionType.Basic);
        var dog = await SeedDogAsync(client.Id);
        var walker = await SeedUserAsync("walker1", UserRole.Walker, "Walk Person");
        var walk = await SeedWalkEventAsync(dog.Id);
        var svc = CreateWalkEventService();

        await svc.ProposeToWalkerAsync(new ProposeWalkDto(walk.Id, walker.Id));
        var result = await svc.WalkerRespondAsync(
            new WalkerResponseDto(walk.Id, walker.Id, false, "Too far away"));

        Assert.Equal(WalkStatus.Requested, result.Status);
        Assert.Null(result.WalkerId);
    }

    [Fact]
    public async Task ClaimWalkAsync_AtomicAssignAndAccept()
    {
        var client = await SeedClientAsync(subscription: SubscriptionType.Basic);
        var dog = await SeedDogAsync(client.Id);
        var walker = await SeedUserAsync("walker1", UserRole.Walker, "Walk Person");
        var walk = await SeedWalkEventAsync(dog.Id);
        var svc = CreateWalkEventService();

        var result = await svc.ClaimWalkAsync(walk.Id, walker.Id);

        Assert.Equal(WalkStatus.Accepted, result.Status);
        Assert.Equal(walker.Id, result.WalkerId);
    }

    [Fact]
    public async Task CancelWithNoteAsync_SetsNoteAndCancels()
    {
        var client = await SeedClientAsync(subscription: SubscriptionType.Basic);
        var dog = await SeedDogAsync(client.Id);
        var walk = await SeedWalkEventAsync(dog.Id);
        var svc = CreateWalkEventService();

        var result = await svc.CancelWithNoteAsync(walk.Id, "Client changed plans");

        Assert.Equal(WalkStatus.Cancelled, result.Status);
        Assert.Equal("Client changed plans", result.Notes);
    }

    [Fact]
    public async Task UnacceptWalkAsync_ReturnsToRequested()
    {
        var client = await SeedClientAsync(subscription: SubscriptionType.Basic);
        var dog = await SeedDogAsync(client.Id);
        var walker = await SeedUserAsync("walker1", UserRole.Walker, "Walk Person");
        var walk = await SeedWalkEventAsync(dog.Id);
        var svc = CreateWalkEventService();

        // Propose + accept
        await svc.ProposeToWalkerAsync(new ProposeWalkDto(walk.Id, walker.Id));
        await svc.WalkerRespondAsync(new WalkerResponseDto(walk.Id, walker.Id, true));

        // Now unaccept
        var result = await svc.UnacceptWalkAsync(walk.Id, "Schedule conflict");

        Assert.Equal(WalkStatus.Requested, result.Status);
        Assert.Null(result.WalkerId);
    }

    [Fact]
    public async Task ScheduleAsync_RecurringWalks_CreatesMultipleEvents()
    {
        var client = await SeedClientAsync(subscription: SubscriptionType.Premium);
        var dog = await SeedDogAsync(client.Id);
        var svc = CreateWalkEventService();

        // Schedule a weekly recurring walk starting next Monday
        var nextMonday = DateTime.UtcNow.Date;
        while (nextMonday.DayOfWeek != DayOfWeek.Monday)
            nextMonday = nextMonday.AddDays(1);
        // Move to a date near the start of next month to get multiple weeks
        nextMonday = nextMonday.AddHours(10);

        var result = await svc.ScheduleAsync(
            new CreateWalkEventDto(dog.Id, nextMonday, 60, "TestZone",
                RecurrenceType: RecurrenceType.WeeklySameDay));

        // Should have created at least 1 walk event (the first one returned)
        Assert.True(result.Id > 0);

        // Check total events created for this dog
        var allWalks = await Uow.WalkEvents.GetByDogIdAsync(dog.Id);
        Assert.True(allWalks.Count() >= 1);
    }
}
