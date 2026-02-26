using DogWalking.Domain.Enums;
using DogWalking.Domain.Exceptions;
using DogWalking.Domain.ValueObjects;

namespace DogWalking.Domain.Entities;

public class Client
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public PhoneNumber PhoneNumber { get; private set; } = null!;
    public string Email { get; private set; } = string.Empty;
    public SubscriptionType Subscription { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>Neighbourhood/zone used to match this client with eligible walkers.</summary>
    public string Zone { get; private set; } = string.Empty;

    /// <summary>Physical street address.</summary>
    public string Address { get; private set; } = string.Empty;

    /// <summary>Optional link to the User account for this client (role = Client).</summary>
    public int? UserId { get; private set; }

    private readonly List<Dog> _dogs = new();
    public IReadOnlyCollection<Dog> Dogs => _dogs.AsReadOnly();

    private Client() { }

    public Client(string name, string phoneNumber, string email,
                  SubscriptionType subscription = SubscriptionType.Free,
                  int? userId = null, string zone = "", string address = "")
    {
        SetName(name);
        SetEmail(email);

        PhoneNumber = new PhoneNumber(phoneNumber);
        Subscription = subscription;
        UserId = userId;
        Zone = zone.Trim();
        Address = address.Trim();
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;

    public void Update(string name, string phoneNumber, string email, string zone = "")
    {
        SetName(name);
        SetEmail(email);

        PhoneNumber = new PhoneNumber(phoneNumber);
        Zone = zone.Trim();
    }

    public void UpdateZone(string zone) => Zone = zone.Trim();

    /// <summary>
    /// Changes the subscription plan for this client.
    /// Downgrading is allowed but does not retroactively cancel existing walks.
    /// </summary>
    public void ChangeSubscription(SubscriptionType newSubscription)
    {
        Subscription = newSubscription;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Client name cannot be empty.");
        if (name.Length > 100)
            throw new DomainException("Client name cannot exceed 100 characters.");
        Name = name.Trim();
    }

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty.");
        if (!email.Contains('@'))
            throw new DomainException("Email is not valid.");
        Email = email.Trim().ToLowerInvariant();
    }
}
