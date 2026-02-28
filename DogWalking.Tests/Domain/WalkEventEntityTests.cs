using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Exceptions;

namespace DogWalking.Tests.Domain;

public class WalkEventEntityTests
{
    private static WalkEvent Valid() =>
        new(dogId: 1, walkDate: DateTime.UtcNow.AddHours(2), durationMinutes: 30, location: "TestZone");

    [Fact]
    public void Constructor_Valid_CreatesRequestedWalk()
    {
        var w = Valid();
        Assert.Equal(WalkStatus.Requested, w.Status);
        Assert.Equal(30, w.DurationMinutes);
    }

    [Fact]
    public void Constructor_PastDate_ThrowsDomainException()
        => Assert.Throws<DomainException>(() =>
            new WalkEvent(1, DateTime.UtcNow.AddHours(-2), 30, "TestZone"));

    [Fact]
    public void Constructor_DurationTooShort_ThrowsDomainException()
        => Assert.Throws<DomainException>(() =>
            new WalkEvent(1, DateTime.UtcNow.AddHours(1), 10, "TestZone"));

    [Fact]
    public void Constructor_DurationTooLong_ThrowsDomainException()
        => Assert.Throws<DomainException>(() =>
            new WalkEvent(1, DateTime.UtcNow.AddHours(1), 600, "TestZone"));

    [Fact]
    public void TransitionTo_RequestedToProposed_Succeeds()
    {
        var w = Valid();
        w.ProposeToWalker(walkerId: 5);
        Assert.Equal(WalkStatus.Proposed, w.Status);
        Assert.Equal(5, w.WalkerId);
    }

    [Fact]
    public void TransitionTo_RequestedToCancelled_Succeeds()
    {
        var w = Valid();
        w.TransitionTo(WalkStatus.Cancelled);
        Assert.Equal(WalkStatus.Cancelled, w.Status);
    }

    [Fact]
    public void FullLifecycle_RequestedToCompleted_Succeeds()
    {
        var w = Valid();
        w.ProposeToWalker(walkerId: 5);
        w.AcceptByWalker();
        w.TransitionTo(WalkStatus.InProgress);
        w.TransitionTo(WalkStatus.Completed);
        Assert.Equal(WalkStatus.Completed, w.Status);
    }

    [Fact]
    public void TransitionTo_CompletedToAny_ThrowsDomainException()
    {
        var w = Valid();
        w.ProposeToWalker(5);
        w.AcceptByWalker();
        w.TransitionTo(WalkStatus.InProgress);
        w.TransitionTo(WalkStatus.Completed);
        Assert.Throws<DomainException>(() => w.TransitionTo(WalkStatus.Requested));
    }

    [Fact]
    public void DeclineByWalker_ClearsWalkerAndGoesToRequested()
    {
        var w = Valid();
        w.ProposeToWalker(5);
        w.DeclineByWalker(5);
        Assert.Equal(WalkStatus.Requested, w.Status);
        Assert.Null(w.WalkerId);
        Assert.Single(w.Declines);
        Assert.Equal(5, w.Declines.First().WalkerId);
    }

    [Fact]
    public void TransitionTo_CancelledToInProgress_ThrowsDomainException()
    {
        var w = Valid();
        w.TransitionTo(WalkStatus.Cancelled);
        Assert.Throws<DomainException>(() => w.TransitionTo(WalkStatus.InProgress));
    }

}