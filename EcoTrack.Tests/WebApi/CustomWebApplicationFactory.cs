using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using EcoTrack.WebApi;

namespace EcoTrack.Tests.WebApi;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var dict = new Dictionary<string, string?>
            {
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
    }
}

