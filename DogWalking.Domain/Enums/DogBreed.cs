namespace DogWalking.Domain.Enums;

public enum DogBreed
{
    LabradorRetriever  = 1,
    GoldenRetriever    = 2,
    GermanShepherd     = 3,
    Bulldog            = 4,
    Poodle             = 5,
    Beagle             = 6,
    Rottweiler         = 7,
    Dachshund          = 8,
    Boxer              = 9,
    SiberianHusky      = 10,
    Other              = 11
}

public static class DogBreedExtensions
{
    public static string ToDisplayName(this DogBreed breed) => breed switch
    {
        DogBreed.LabradorRetriever => "Labrador Retriever",
        DogBreed.GoldenRetriever   => "Golden Retriever",
        DogBreed.GermanShepherd    => "German Shepherd",
        DogBreed.Bulldog           => "Bulldog",
        DogBreed.Poodle            => "Poodle",
        DogBreed.Beagle            => "Beagle",
        DogBreed.Rottweiler        => "Rottweiler",
        DogBreed.Dachshund         => "Dachshund",
        DogBreed.Boxer             => "Boxer",
        DogBreed.SiberianHusky     => "Siberian Husky",
        DogBreed.Other             => "Other",
        _                          => breed.ToString()
    };

    public static IEnumerable<DogBreed> All() => Enum.GetValues<DogBreed>();
}
