using DogWalking.Application.DTOs;

namespace DogWalking.Application.Interfaces;

public interface IAiWalkParserService
{
    /// <summary>
    /// Parses a natural language request into structured walk data.
    /// </summary>
    /// <param name="input">Free-text input from the user.</param>
    /// <param name="existingDogNames">Names of dogs the client already owns.</param>
    /// <param name="defaultZone">Client's default zone — used when no location is specified.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<AiParsedWalkRequest> ParseAsync(string input, IEnumerable<string> existingDogNames,
        string defaultZone, CancellationToken ct = default);
}
