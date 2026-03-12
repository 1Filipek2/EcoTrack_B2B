using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Xunit;
using EcoTrack.WebApi;

namespace EcoTrack.Tests.WebApi;

public class ErrorHandlingMiddlewareIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ErrorHandlingMiddlewareIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var dict = new Dictionary<string, string?>
                {
                    {"JwtSettings:SecretKey", "test-secret-key-1234567890-abcdefghijklmnopqrstuvwxyz"},
                    {"JwtSettings:Issuer", "EcoTrackAPI"},
                    {"JwtSettings:Audience", "EcoTrackClient"}
                };
                config.AddInMemoryCollection(dict);
            });
        });
    }

    [Fact]
    public async Task InvalidEndpoint_Returns500Or404()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/invalid-endpoint");
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.NotFound || response.StatusCode == System.Net.HttpStatusCode.InternalServerError);
    }
}
