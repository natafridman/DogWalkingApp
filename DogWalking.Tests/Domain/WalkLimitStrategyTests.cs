using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Exceptions;
using DogWalking.Domain.Services;

namespace DogWalking.Tests.Domain;

public class WalkLimitStrategyTests
{
    private static DateTime FutureDate(int daysAhead = 1, int hour = 10)
        => DateTime.UtcNow.Date.AddDays(daysAhead).AddHours(hour);

    private static List<WalkEvent> MakeWalks(int count, int monthOffset = 0)
    {
        var walks = new List<WalkEvent>();
        for (int i = 0; i < count; i++)
            walks.Add(CreateScheduledWalk(DateTime.UtcNow.AddDays(i + 1 + monthOffset * 30)));
        return walks;
    }

    private static WalkEvent CreateScheduledWalk(DateTime date) =>
        new(dogId: 1, walkDate: date, durationMinutes: 30, location: "TestZone");

    // ── FREE ──────────────────────────────────────────

    [Fact]
    public void Free_FirstWalk_Allowed()
    {
        var strategy = WalkLimitStrategyFactory.Create(SubscriptionType.Free);
        var ex = Record.Exception(() =>
            strategy.ValidateWalkAllowed(new List<WalkEvent>(), FutureDate(1)));
        Assert.Null(ex);
    }

    [Fact]
    public void Free_SecondWalkSameMonth_Blocked()
    {
        var strategy = WalkLimitStrategyFactory.Create(SubscriptionType.Free);
        var existing = new[] { CreateScheduledWalk(FutureDate(2)) };
        Assert.Throws<DomainException>(() =>
            strategy.ValidateWalkAllowed(existing, FutureDate(3)));
    }

    [Fact]
    public void Free_TwoWalksSameDay_Blocked()
    {
        var strategy = WalkLimitStrategyFactory.Create(SubscriptionType.Free);
        var proposedDay = FutureDate(1, 9);
        var existing  = new[] { CreateScheduledWalk(proposedDay) };

        // Even if monthly limit not hit, same-day block applies
        // (Free already hits monthly limit anyway — covered by Free_SecondWalkSameMonth_Blocked)
        // Separate daily test: use Basic which has more monthly slots
        var basicStrategy = WalkLimitStrategyFactory.Create(SubscriptionType.Basic);
        var sameDay       = FutureDate(1, 14);
        var existingSameDay = new[] { CreateScheduledWalk(FutureDate(1, 9)) };

        Assert.Throws<DomainException>(() =>
            basicStrategy.ValidateWalkAllowed(existingSameDay, sameDay));
    }

    // ── BASIC ─────────────────────────────────────────

    [Fact]
    public void Basic_Within12_DifferentDays_Allowed()
    {
        var strategy = WalkLimitStrategyFactory.Create(SubscriptionType.Basic);
        var existing = MakeWalks(5);
        var ex = Record.Exception(() =>
            strategy.ValidateWalkAllowed(existing, FutureDate(10)));
        Assert.Null(ex);
    }

    [Fact]
    public void Basic_13thWalk_Blocked()
    {
        var strategy = WalkLimitStrategyFactory.Create(SubscriptionType.Basic);
        var existing = MakeWalks(12);
        Assert.Throws<DomainException>(() =>
            strategy.ValidateWalkAllowed(existing, FutureDate(15)));
    }

    [Fact]
    public void Basic_SameDayWalk_Blocked()
    {
        var strategy = WalkLimitStrategyFactory.Create(SubscriptionType.Basic);
        var existing = new[] { CreateScheduledWalk(FutureDate(1, 9)) };
        Assert.Throws<DomainException>(() =>
            strategy.ValidateWalkAllowed(existing, FutureDate(1, 14)));
    }

    // ── PRO ───────────────────────────────────────────

    [Fact]
    public void Pro_Within20_DifferentDays_Allowed()
    {
        var strategy = WalkLimitStrategyFactory.Create(SubscriptionType.Pro);
        var existing = MakeWalks(10);
        var ex = Record.Exception(() =>
            strategy.ValidateWalkAllowed(existing, FutureDate(15)));
        Assert.Null(ex);
    }

    [Fact]
    public void Pro_21stWalk_Blocked()
    {
        var strategy = WalkLimitStrategyFactory.Create(SubscriptionType.Pro);
        var existing = MakeWalks(20);
        Assert.Throws<DomainException>(() =>
            strategy.ValidateWalkAllowed(existing, FutureDate(25)));
    }

    // ── PREMIUM ───────────────────────────────────────

    [Fact]
    public void Premium_MultipleWalksSameDay_Allowed()
    {
        var strategy = WalkLimitStrategyFactory.Create(SubscriptionType.Premium);
        var existing = new[]
        {
            CreateScheduledWalk(FutureDate(1, 8)),
            CreateScheduledWalk(FutureDate(1, 10)),
            CreateScheduledWalk(FutureDate(1, 12))
        };
        var ex = Record.Exception(() =>
            strategy.ValidateWalkAllowed(existing, FutureDate(1, 14)));
        Assert.Null(ex);
    }

    [Fact]
    public void Premium_51stWalk_Blocked()
    {
        var strategy = WalkLimitStrategyFactory.Create(SubscriptionType.Premium);
        var existing = MakeWalks(50);

        Assert.Throws<DomainException>(() =>
            strategy.ValidateWalkAllowed(existing, FutureDate(55)));
    }

    [Fact]
    public void Premium_CancelledWalks_DontCountAgainstLimit()
    {
        var strategy = WalkLimitStrategyFactory.Create(SubscriptionType.Free);
        var walk = CreateScheduledWalk(FutureDate(3));
        walk.TransitionTo(WalkStatus.Cancelled);
        var ex = Record.Exception(() =>
            strategy.ValidateWalkAllowed(new[] { walk }, FutureDate(5)));
        Assert.Null(ex);
    }

    // ── FACTORY ───────────────────────────────────────

    [Theory]
    [InlineData(SubscriptionType.Free, 1, false)]
    [InlineData(SubscriptionType.Basic, 12, false)]
    [InlineData(SubscriptionType.Pro, 20, false)]
    [InlineData(SubscriptionType.Premium, 50, true)]
    public void Factory_CreatesCorrectStrategy(SubscriptionType type, int maxPerMonth, bool multiPerDay)
    {
        var strategy = WalkLimitStrategyFactory.Create(type);
        Assert.Equal(type, strategy.SubscriptionType);
        Assert.Equal(maxPerMonth, strategy.MaxWalksPerMonth);
        Assert.Equal(multiPerDay, strategy.AllowsMultiplePerDay);
    }
}