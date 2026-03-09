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

// Get raw connection string from config
var rawConnectionString = builder.Configuration.GetConnectionString("EcoTrackDatabase");

// CRITICAL: Normalize connection string BEFORE DI registration
// Railway may encode it with quotes, extra params, incomplete sslmode, etc.
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

// Apply migrations and seed data (with error handling for Railway startup)
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

    var normalized = value
        .Trim()
        .Trim('"')           // Remove surrounding quotes Railway may add
        .Trim('\'');         // Remove single quotes too

    // Remove malformed channel_binding parameter that Npgsql doesn't recognize
    normalized = System.Text.RegularExpressions.Regex.Replace(
        normalized,
        @"[&?]channel_binding=\w+",
        "",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase
    );

    // Fix incomplete sslmode (ends with ?sslmode without value)
    if (normalized.EndsWith("?sslmode", StringComparison.OrdinalIgnoreCase))
    {
        normalized += "=require";
    }

    // Ensure sslmode=require is present
    if (!normalized.Contains("sslmode", StringComparison.OrdinalIgnoreCase))
    {
        // If no sslmode at all, add it
        normalized += normalized.Contains("?", StringComparison.OrdinalIgnoreCase) ? "&sslmode=require" : "?sslmode=require";
    }

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
