using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DogWalking.Application.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DogWalking.Api.Configuration;

/// <summary>
/// Generates signed JWT tokens from authenticated user data.
/// Injected via DI so the controller stays thin.
/// </summary>
public sealed class JwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public (string Token, int ExpiresIn) Generate(AuthResultDto auth)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, auth.UserId!.Value.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, auth.Username ?? ""),
            new Claim(ClaimTypes.Role, auth.Role ?? ""),
            new Claim("full_name", auth.FullName ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpiresInMinutes),
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), _settings.ExpiresInMinutes * 60);
    }
}
