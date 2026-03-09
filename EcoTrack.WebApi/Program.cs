using EcoTrack.Application;
using EcoTrack.Core.Entities;
using EcoTrack.Core.Enums;
using EcoTrack.Infrastructure;
using EcoTrack.Infrastructure.Persistence;
using EcoTrack.WebApi.Configuration;
using EcoTrack.WebApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var originsConfig = builder.Configuration["CorsOrigins"];
        var origins = !string.IsNullOrWhiteSpace(originsConfig)
            ? originsConfig.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : new[] { "http://localhost:3000", "http://localhost:4200", "http://localhost:5173" };

        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// JWT Authentication (fail fast if secret is missing in production)
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

if (string.IsNullOrWhiteSpace(secretKey))
    throw new InvalidOperationException("JwtSettings:SecretKey is required.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "EcoTrackAPI",
            ValidAudience = jwtSettings["Audience"] ?? "EcoTrackClient",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddApplication();

var rawConnectionString = builder.Configuration.GetConnectionString("EcoTrackDatabase")
    ?? builder.Configuration["ConnectionStrings__EcoTrackDatabase"];

var normalizedConnectionString = string.IsNullOrWhiteSpace(rawConnectionString)
    ? "Host=localhost;Port=5432;Database=placeholder;Username=placeholder;Password=placeholder;SSL Mode=Disable"
    : ConnectionStringNormalizer.Normalize(rawConnectionString);

builder.Services.AddInfrastructure(normalizedConnectionString);

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();

try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<EcoTrackDbContext>();
    await dbContext.Database.MigrateAsync();
    await SeedEmissionCategoriesAsync(dbContext);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to run migrations or seed data. App will continue without DB.");
}

app.UseCors("AllowFrontend");

// Redirect HTTPS only outside cloud reverse-proxy scenarios.
if (!app.Environment.IsProduction())
    app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => "Hi!");

app.Run();

static async Task SeedEmissionCategoriesAsync(EcoTrackDbContext dbContext)
{
    if (await dbContext.EmissionCategories.AnyAsync())
        return;

    var defaults = new[]
    {
        new EmissionCategory { Name = "Electricity", Description = "Purchased electricity consumption (kWh).", Scope = EmissionScope.Scope2 },
        new EmissionCategory { Name = "Natural Gas", Description = "Natural gas used for heating/processes (m3).", Scope = EmissionScope.Scope1 },
        new EmissionCategory { Name = "Fleet Fuel", Description = "Diesel/petrol consumed by company-owned vehicles.", Scope = EmissionScope.Scope1 },
        new EmissionCategory { Name = "Business Travel", Description = "Air, rail, taxi and hotel stays from business travel.", Scope = EmissionScope.Scope3 },
        new EmissionCategory { Name = "Employee Commuting", Description = "Daily commute emissions of employees.", Scope = EmissionScope.Scope3 },
        new EmissionCategory { Name = "Waste Disposal", Description = "Waste treatment and disposal emissions.", Scope = EmissionScope.Scope3 },
        new EmissionCategory { Name = "Water Consumption", Description = "Water supply and wastewater treatment footprint.", Scope = EmissionScope.Scope3 },
        new EmissionCategory { Name = "Purchased Goods", Description = "Upstream emissions from purchased materials/services.", Scope = EmissionScope.Scope3 }
    };

    dbContext.EmissionCategories.AddRange(defaults);
    await dbContext.SaveChangesAsync(CancellationToken.None);
}
