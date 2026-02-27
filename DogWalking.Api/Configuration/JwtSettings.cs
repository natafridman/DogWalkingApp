namespace DogWalking.Api.Configuration;

/// <summary>
/// Strongly-typed JWT settings bound from appsettings.json section "Jwt".
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpiresInMinutes { get; init; } = 60;
}
