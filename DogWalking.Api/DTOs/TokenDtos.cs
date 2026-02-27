namespace DogWalking.Api.DTOs;

public record TokenRequest(string Username, string Password);

public record TokenResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string Role);
