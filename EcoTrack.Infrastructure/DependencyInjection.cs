using EcoTrack.Application.Interfaces;
using EcoTrack.Infrastructure.Persistence;
using EcoTrack.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EcoTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<EcoTrackDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.UseVector();
            }));

        services.AddScoped<IEcoTrackDbContext>(provider => provider.GetRequiredService<EcoTrackDbContext>());
        services.AddScoped<IAiExtractorService, AiExtractorService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}