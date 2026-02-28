namespace DogWalking.Application.DTOs;

public record DogDto(
    int      Id,
    int      ClientId,
    string   ClientName,
    string   Name,
    string   Breed,
    DateOnly BirthDate
)
{
    public int AgeInYears =>
        (DateOnly.FromDateTime(DateTime.Today).DayNumber - BirthDate.DayNumber) / 365;
}

public record CreateDogDto(
    int      ClientId,
    string   Name,
    string   Breed,
    DateOnly BirthDate
);

public record UpdateDogDto(
    int      Id,
    string   Name,
    string   Breed,
    DateOnly BirthDate
);
