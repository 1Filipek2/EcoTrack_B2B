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

public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
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
    public async Task Register_And_Login_Works()
    {
        var client = _factory.CreateClient();
        var registerPayload = new
        {
            email = "testuser@example.com",
            password = "TestPassword123!",
            companyName = "Test Company"
        };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerPayload);
        Assert.True(registerResponse.IsSuccessStatusCode);

        var loginPayload = new
        {
            email = "testuser@example.com",
            password = "TestPassword123!"
        };
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginPayload);
        Assert.True(loginResponse.IsSuccessStatusCode);
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        Assert.Contains("token", loginContent);
    }

    [Fact]
    public async Task Register_With_Invalid_Email_Returns_BadRequest()
    {
        var client = _factory.CreateClient();
        var payload = new
        {
            email = "invalid-email",
            password = "TestPassword123!",
            companyName = "Test Company"
        };
        var response = await client.PostAsJsonAsync("/api/auth/register", payload);
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
