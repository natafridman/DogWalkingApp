using DogWalking.Api.Configuration;
using DogWalking.Api.Validators;
using DogWalking.Infrastructure.Extensions;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// ── Domain + Infrastructure services
var conn = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Missing 'Default' connection string.");

builder.Services.AddDogWalkingServices(conn);

// ── JWT authentication
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSingleton<JwtTokenService>();

// ── Validation
builder.Services.AddValidatorsFromAssemblyContaining<TokenRequestValidator>();

// ── Controllers + OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Middleware pipeline
if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ── Database initialization
await app.Services.InitializeDatabaseAsync();

app.Run();
