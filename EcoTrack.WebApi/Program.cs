using EcoTrack.Application;
using EcoTrack.Core.Entities;
using EcoTrack.Core.Enums;
using EcoTrack.Infrastructure;
using EcoTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        // Get origins from config or use defaults for development
        var originsConfig = builder.Configuration["CorsOrigins"];
        var origins = !string.IsNullOrEmpty(originsConfig)
            ? originsConfig.Split(',', StringSplitOptions.RemoveEmptyEntries)
            : new[] { "http://localhost:3000", "http://localhost:4200", "http://localhost:5173" };

        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-min-32-chars-long-for-HS256";

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

var rawConnectionString = builder.Configuration.GetConnectionString("EcoTrackDatabase");

var normalizedConnectionString = string.IsNullOrEmpty(rawConnectionString)
    ? "Host=localhost;Database=placeholder;Username=placeholder;Password=placeholder"
    : NormalizeConnectionString(rawConnectionString);

builder.Services.AddInfrastructure(normalizedConnectionString);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<EcoTrackDbContext>();
        await dbContext.Database.MigrateAsync();
        await SeedEmissionCategoriesAsync(dbContext);
    }
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to run migrations or seed data. App will continue without DB.");
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.MapGet("/", () => "Hi!");

app.Run();

static string NormalizeConnectionString(string value)
{
    if (string.IsNullOrWhiteSpace(value))
        return "Host=localhost;Database=placeholder;Username=placeholder;Password=placeholder";

    var normalized = value.Trim().Trim('"').Trim('\'');
    
    if (normalized.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
        normalized.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
    {
        if (Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
        {
            var userInfo = uri.UserInfo?.Split(':', 2, StringSplitOptions.None) ?? Array.Empty<string>();
            var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
            var database = uri.AbsolutePath.Trim('/');
            var port = uri.IsDefaultPort ? 5432 : uri.Port;

            if (!string.IsNullOrWhiteSpace(uri.Host) && !string.IsNullOrWhiteSpace(database))
            {
                return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
            }
        }
    }

    // Fallback for key-value format; strip unsupported channel_binding parameter.
    normalized = System.Text.RegularExpressions.Regex.Replace(
        normalized,
        @"[&?]channel_binding=\w+",
        "",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase
    );

    if (normalized.EndsWith("?sslmode", StringComparison.OrdinalIgnoreCase))
        normalized += "=require";

    if (!normalized.Contains("sslmode", StringComparison.OrdinalIgnoreCase) &&
        !normalized.Contains("SSL Mode", StringComparison.OrdinalIgnoreCase))
        normalized += normalized.Contains("?", StringComparison.OrdinalIgnoreCase) ? "&sslmode=require" : ";SSL Mode=Require";

    return normalized;
}

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
