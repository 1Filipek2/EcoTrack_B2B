using EcoTrack.WebApi.Configuration;
using Microsoft.Extensions.Configuration;

namespace EcoTrack.Tests.WebApi;

public class ConnectionStringNormalizerTests
{
    static ConnectionStringNormalizerTests()
    {
        var dict = new Dictionary<string, string?>
        {
            {"JwtSettings:SecretKey", "test-secret-key-1234567890-abcdefghijklmnopqrstuvwxyz"},
            {"JwtSettings:Issuer", "EcoTrackAPI"},
            {"JwtSettings:Audience", "EcoTrackClient"}
        };
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(dict);
        var config = configBuilder.Build();
        // Optionally set as global config if needed
    }

    [Fact]
    public void Normalize_LocalhostKeyValue_DisablesSsl()
    {
        var input = "Host=localhost;Port=55432;Database=EcoTrackDb;Username=postgres;Password=postgres";

        var result = ConnectionStringNormalizer.Normalize(input);

        Assert.Contains("SSL Mode=Disable", result);
        Assert.DoesNotContain("SSL Mode=Require", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Normalize_RemoteUri_RequiresSsl()
    {
        var input = "postgresql://user:pass@db.example.com:5432/neondb?sslmode=require&channel_binding=require";

        var result = ConnectionStringNormalizer.Normalize(input);

        Assert.Contains("Host=db.example.com", result);
        Assert.Contains("Database=neondb", result);
        Assert.Contains("SSL Mode=Require", result);
        Assert.Contains("Trust Server Certificate=true", result);
    }

    [Fact]
    public void Normalize_RemovesChannelBinding()
    {
        var input = "Host=db.example.com;Database=EcoTrackDb;Username=u;Password=p;SSL Mode=Require;channel_binding=require";

        var result = ConnectionStringNormalizer.Normalize(input);

        Assert.DoesNotContain("channel_binding", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Normalize_LocalhostUri_DisablesSsl()
    {
        var input = "postgresql://user:pass@localhost:55432/EcoTrackDb";

        var result = ConnectionStringNormalizer.Normalize(input);

        Assert.Contains("Host=localhost", result);
        Assert.Contains("Port=55432", result);
        Assert.Contains("SSL Mode=Disable", result);
    }
}
