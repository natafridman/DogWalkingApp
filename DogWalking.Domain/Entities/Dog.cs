using DogWalking.Domain.Enums;
using DogWalking.Domain.Exceptions;

namespace DogWalking.Domain.Entities;

/// <summary>
/// Represents a dog belonging to a client.
/// </summary>
public class Dog
{
    public int Id { get; private set; }
    public int ClientId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Breed { get; private set; } = string.Empty;
    public DateOnly BirthDate { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public int AgeInYears =>
        (DateOnly.FromDateTime(DateTime.Today).DayNumber - BirthDate.DayNumber) / 365;

    public Client Client { get; private set; } = null!;

    private Dog() { }

    public Dog(int clientId, string name, string breed, DateOnly birthDate)
    {
        ClientId = clientId;
        SetName(name);
        SetBreed(breed);
        SetBirthDate(birthDate);
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string breed, DateOnly birthDate)
    {
        SetName(name);
        SetBreed(breed);
        SetBirthDate(birthDate);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Dog name cannot be empty.");
        if (name.Length > 80)
            throw new DomainException("Dog name cannot exceed 80 characters.");
        Name = name.Trim();
    }

    private void SetBreed(string breed)
    {
        if (string.IsNullOrWhiteSpace(breed))
            throw new DomainException("Breed cannot be empty.");
        Breed = breed.Trim();
    }

    private void SetBirthDate(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (birthDate > today)
            throw new DomainException("Birth date cannot be in the future.");
        if (birthDate < today.AddYears(-30))
            throw new DomainException("Dog age cannot exceed 30 years.");
        BirthDate = birthDate;
    }
}
