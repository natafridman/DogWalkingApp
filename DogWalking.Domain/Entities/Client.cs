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

    public string Zone { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
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

    /// <summary>Throws if the client has active walks.</summary>
    public void Deactivate()
    {
        bool hasActiveWalks = _dogs
            .SelectMany(d => d.WalkEvents)
            .Any(w => w.Status is WalkStatus.Requested or WalkStatus.Proposed
                               or WalkStatus.Accepted  or WalkStatus.InProgress);

        if (hasActiveWalks)
            throw new DomainException(
                "Cannot deactivate a client who has active or in-progress walks.");

        IsActive = false;
    }

    public void Update(string name, string phoneNumber, string email, string zone = "")
    {
        SetName(name);
        SetEmail(email);

        PhoneNumber = new PhoneNumber(phoneNumber);
        Zone = zone.Trim();
    }

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
