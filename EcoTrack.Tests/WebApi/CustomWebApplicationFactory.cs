using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using EcoTrack.WebApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EcoTrack.Tests.WebApi;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    private Microsoft.Data.Sqlite.SqliteConnection _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var dict = new Dictionary<string, string?>
            {
                {"ConnectionStrings:EcoTrackDatabase", "DataSource=:memory:"},
                {"JwtSettings:SecretKey", "test-secret-key-1234567890-abcdefghijklmnopqrstuvwxyz"},
                {"JwtSettings:Issuer", "EcoTrackAPI"},
                {"JwtSettings:Audience", "EcoTrackClient"},
                {"SendGrid:ApiKey", ""},
                {"SendGrid:FromEmail", "test@ecotrack.com"},
                {"SendGrid:FromName", "EcoTrack Test"},
                {"ASPNETCORE_ENVIRONMENT", "Development"}
            };
            config.AddInMemoryCollection(dict);
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(Microsoft.EntityFrameworkCore.DbContextOptions<EcoTrack.Infrastructure.Persistence.EcoTrackDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);
            
            _connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
            _connection.Open();
            
            services.AddDbContext<EcoTrack.Infrastructure.Persistence.EcoTrackDbContext>(options =>
                options.UseSqlite(_connection));
            
            services.AddScoped<EcoTrack.Application.Interfaces.IEmailService, FakeEmailService>();
            
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EcoTrack.Infrastructure.Persistence.EcoTrackDbContext>();
                db.Database.EnsureCreated();
            }
        });
    }
}

