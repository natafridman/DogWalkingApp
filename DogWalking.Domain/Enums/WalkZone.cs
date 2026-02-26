namespace DogWalking.Domain.Enums;

/// <summary>Predefined geographic zones used for walk location and walker matching.</summary>
public enum WalkZone
{
    VillaCrespo  = 1,
    Palermo      = 2,
    Recoleta     = 3,
    Retiro       = 4,
    PuertoMadero = 5
}

public static class WalkZoneExtensions
{
    public static string ToDisplayName(this WalkZone zone) => zone switch
    {
        WalkZone.VillaCrespo  => "Villa Crespo",
        WalkZone.Palermo      => "Palermo",
        WalkZone.Recoleta     => "Recoleta",
        WalkZone.Retiro       => "Retiro",
        WalkZone.PuertoMadero => "Puerto Madero",
        _                     => zone.ToString()
    };

    public static IEnumerable<WalkZone> All() => Enum.GetValues<WalkZone>();
}
