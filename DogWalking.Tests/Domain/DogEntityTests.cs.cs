using DogWalking.Domain.Entities;
using DogWalking.Domain.Exceptions;

namespace DogWalking.Tests.Domain;

public class DogEntityTests
{
    private static DateOnly BirthDate(int yearsAgo) =>
        DateOnly.FromDateTime(DateTime.Today.AddYears(-yearsAgo));

    [Fact]
    public void Constructor_ValidData_CreatesDog()
    {
        var d = new Dog(1, "Rex", "Labrador", BirthDate(3));
        Assert.Equal("Rex", d.Name);
        Assert.Equal("Labrador", d.Breed);
        Assert.Equal(3, d.AgeInYears);
    }

    [Fact]
    public void Constructor_EmptyName_ThrowsDomainException()
        => Assert.Throws<DomainException>(() => new Dog(1, "", "Labrador", BirthDate(3)));

    [Fact]
    public void Constructor_EmptyBreed_ThrowsDomainException()
        => Assert.Throws<DomainException>(() => new Dog(1, "Rex", "", BirthDate(3)));

    [Fact]
    public void Constructor_FutureBirthDate_ThrowsDomainException()
        => Assert.Throws<DomainException>(() => new Dog(1, "Rex", "Lab", DateOnly.FromDateTime(DateTime.Today.AddDays(1))));

    [Fact]
    public void Constructor_TooOldBirthDate_ThrowsDomainException()
        => Assert.Throws<DomainException>(() => new Dog(1, "Rex", "Lab", DateOnly.FromDateTime(DateTime.Today.AddYears(-31))));

    [Fact]
    public void ValidateNoConflictingWalk_NoExistingWalks_DoesNotThrow()
    {
        var d = new Dog(1, "Rex", "Lab", BirthDate(3));
        var ex = Record.Exception(() =>
            d.ValidateNoConflictingWalk(DateTime.UtcNow.AddHours(1), 30));
        Assert.Null(ex);
    }

    [Fact]
    public void Update_ValidData_UpdatesProperties()
    {
        var d = new Dog(1, "Rex", "Lab", BirthDate(3));
        d.Update("Max", "Poodle", BirthDate(5));
        Assert.Equal("Max", d.Name);
        Assert.Equal("Poodle", d.Breed);
        Assert.Equal(5, d.AgeInYears);
    }
}