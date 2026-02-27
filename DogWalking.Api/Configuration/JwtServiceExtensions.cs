using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace DogWalking.Api.Configuration;

/// <summary>
/// Extracts all JWT wiring out of Program.cs into a focused extension method.
/// </summary>
public static class JwtServiceExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(JwtSettings.SectionName);
        services.Configure<JwtSettings>(section);

        var settings = section.Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt settings section is missing or invalid.");

        var key = Encoding.UTF8.GetBytes(settings.Key);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = settings.Issuer,
                    ValidAudience            = settings.Audience,
                    IssuerSigningKey         = new SymmetricSecurityKey(key)
                };
            });

        services.AddAuthorization();

        return services;
    }
}
