using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Exceptions;

namespace DogWalking.Tests.Domain;

public class ClientEntityTests
{
    [Fact]
    public void Constructor_ValidData_CreatesActiveClientWithFreeSubscription()
    {
        var c = new Client("John Doe", "+1234567890", "john@example.com");
        Assert.Equal("John Doe", c.Name);
        Assert.Equal("john@example.com", c.Email);
        Assert.Equal(SubscriptionType.Free, c.Subscription);
        Assert.True(c.IsActive);
    }

    [Fact]
    public void Constructor_WithSubscription_SetsCorrectPlan()
    {
        var c = new Client("Jane", "+1234567890", "j@j.com", SubscriptionType.Premium);
        Assert.Equal(SubscriptionType.Premium, c.Subscription);
    }

    [Fact]
    public void Constructor_EmptyName_ThrowsDomainException()
        => Assert.Throws<DomainException>(() => new Client("", "+1234567890", "a@b.com"));

    [Fact]
    public void Constructor_NameTooLong_ThrowsDomainException()
        => Assert.Throws<DomainException>(() =>
            new Client(new string('A', 101), "+1234567890", "a@b.com"));

    [Fact]
    public void Constructor_InvalidEmail_ThrowsDomainException()
        => Assert.Throws<DomainException>(() => new Client("John", "+1234567890", "bademail"));

    [Fact]
    public void ChangeSubscription_UpdatesPlan()
    {
        var c = new Client("John", "+1234567890", "j@j.com");
        c.ChangeSubscription(SubscriptionType.Pro);
        Assert.Equal(SubscriptionType.Pro, c.Subscription);
    }

    [Fact]
    public void Deactivate_NoActiveWalks_Deactivates()
    {
        var c = new Client("John", "+1234567890", "j@j.com");
        c.Deactivate();
        Assert.False(c.IsActive);
    }

    [Fact]
    public void Update_ValidData_UpdatesAllFields()
    {
        var c = new Client("Old", "+1111111111", "old@old.com");
        c.Update("New", "+9999999999", "new@new.com");
        Assert.Equal("New", c.Name);
        Assert.Equal("new@new.com", c.Email);
    }
}