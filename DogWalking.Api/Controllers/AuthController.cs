using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DogWalking.Api.Configuration;
using DogWalking.Api.DTOs;
using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DogWalking.Api.Controllers;

/// <summary>
/// OAuth2-style authentication controller.
/// Provides a token endpoint (RFC 6749 §4.3 — Resource Owner Password Credentials)
/// and a protected endpoint that returns the authenticated user's claims.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly JwtTokenService _jwt;
    private readonly IValidator<TokenRequest> _validator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService auth,
        JwtTokenService jwt,
        IValidator<TokenRequest> validator,
        ILogger<AuthController> logger)
    {
        _auth = auth;
        _jwt = jwt;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Exchange credentials for a JWT access_token.
    /// </summary>
    [HttpPost("token")]
    [AllowAnonymous]
    public async Task<IActionResult> Token([FromBody] TokenRequest request, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Token request validation failed for user '{Username}'", request.Username);
            return BadRequest(new
            {
                error = "invalid_request",
                error_description = validation.Errors.First().ErrorMessage
            });
        }

        var result = await _auth.LoginAsync(new LoginDto(request.Username, request.Password), ct);

        if (!result.Success || result.UserId is null)
        {
            _logger.LogWarning("Failed login attempt for user '{Username}'", request.Username);
            return Unauthorized(new
            {
                error = "invalid_grant",
                error_description = result.ErrorMessage ?? "Invalid credentials."
            });
        }

        var (token, expiresIn) = _jwt.Generate(result);

        _logger.LogInformation("Token issued for user '{Username}' (Role: {Role})",
            result.Username, result.Role);

        return Ok(new TokenResponse(token, "Bearer", expiresIn, result.Role ?? ""));
    }

    /// <summary>
    /// Returns the authenticated user's claims extracted from the JWT.
    /// </summary>
    [HttpGet("/api/me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub),
            username = User.FindFirstValue(JwtRegisteredClaimNames.UniqueName),
            role = User.FindFirstValue(ClaimTypes.Role),
            fullName = User.FindFirstValue("full_name")
        });
    }
}
