using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost; 
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EcoTrack.WebApi;
using EcoTrack.Infrastructure.Persistence;
using EcoTrack.Application.Interfaces; 

namespace EcoTrack.Tests.WebApi;

public class FakeEmailService : IEmailService
{
    public Task<bool> SendVerificationEmailAsync(string email, string verificationCode, CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public Task<bool> SendPasswordResetEmailAsync(string email, string resetLink, CancellationToken cancellationToken = default)
        => Task.FromResult(true);
}

public class AuthControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public AuthControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_And_Login_Works()
    {
        var client = _factory.CreateClient();
        var email = $"testuser_{Guid.NewGuid()}@example.com";
        var password = "TestPassword123!";
        
        var registerPayload = new { email, password };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerPayload);
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        Assert.True(registerResponse.IsSuccessStatusCode, $"Register failed: {registerContent}");
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcoTrackDbContext>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            Assert.NotNull(user);
            user.IsEmailVerified = true;
            await db.SaveChangesAsync();
        }
        
        var loginPayload = new { email, password };
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginPayload);
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        Assert.True(loginResponse.IsSuccessStatusCode, $"Login failed: {loginContent}");
        Assert.Contains("token", loginContent);
    }

    [Fact]
    public async Task Register_With_CompanyId_Works()
    {
        var client = _factory.CreateClient();
        var email = $"testuser_{Guid.NewGuid()}@example.com";
        var password = "TestPassword123!";
        Guid companyId;
        
        var uniqueVat = Guid.NewGuid().ToString();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcoTrackDbContext>();
            var company = new EcoTrack.Core.Entities.Company { Name = "TestCompany", VatNumber = uniqueVat };
            db.Companies.Add(company);
            await db.SaveChangesAsync();
            companyId = company.Id; 
        }
        
        var registerPayload = new { email, password, companyId };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerPayload);
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        Assert.True(registerResponse.IsSuccessStatusCode, $"Register failed: {registerContent}");
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcoTrackDbContext>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            Assert.NotNull(user);
            user.IsEmailVerified = true;
            await db.SaveChangesAsync();
        }
        
        var loginPayload = new { email, password };
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginPayload);
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        Assert.True(loginResponse.IsSuccessStatusCode, $"Login failed: {loginContent}");
        Assert.Contains("token", loginContent);
    }

    [Fact]
    public async Task Register_With_Invalid_Email_Returns_BadRequest()
    {
        var client = _factory.CreateClient();
        var payload = new { email = "invalid-email", password = "TestPassword123!" };
        var response = await client.PostAsJsonAsync("/api/auth/register", payload);
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}