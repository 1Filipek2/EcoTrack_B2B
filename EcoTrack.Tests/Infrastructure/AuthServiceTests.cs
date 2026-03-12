using System.Security.Cryptography;
using System.Text;
using EcoTrack.Application.Interfaces;
using EcoTrack.Core.Entities;
using EcoTrack.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EcoTrack.Tests.Infrastructure;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_WhenEmailSendFails_RollsBackUser()
    {
        await using var db = CreateDbContext();
        var email = new FakeEmailService { VerificationResult = false };
        var sut = CreateAuthService(db, email);

        var result = await sut.RegisterAsync("rollback@test.com", "Test123!", null, null, null, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("Unable to send verification email", result.Message);
        Assert.Empty(db.Users);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailSendSucceeds_CreatesUnverifiedUser()
    {
        await using var db = CreateDbContext();
        var email = new FakeEmailService { VerificationResult = true };
        var sut = CreateAuthService(db, email);

        var result = await sut.RegisterAsync("new@test.com", "Test123!", null, null, null, CancellationToken.None);

        Assert.True(result.Success);
        var user = await db.Users.SingleAsync();
        Assert.False(user.IsEmailVerified);
        Assert.NotNull(user.EmailVerificationToken);
        Assert.NotNull(user.EmailVerificationTokenExpiresAt);
        Assert.Equal("new@test.com", email.LastEmail);
        Assert.False(string.IsNullOrWhiteSpace(email.LastVerificationCode));
    }

    [Fact]
    public async Task LoginAsync_UnverifiedUser_ReturnsFailure()
    {
        await using var db = CreateDbContext();
        db.Users.Add(new User
        {
            Email = "unverified@test.com",
            PasswordHash = Hash("Test123!"),
            IsEmailVerified = false,
            Role = "Admin"
        });
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = CreateAuthService(db, new FakeEmailService());

        var result = await sut.LoginAsync("unverified@test.com", "Test123!", CancellationToken.None);

        Assert.False(result.Success);
        Assert.True(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task VerifyEmailAsync_WithSentCode_VerifiesUser()
    {
        await using var db = CreateDbContext();
        var email = new FakeEmailService { VerificationResult = true };
        var sut = CreateAuthService(db, email);

        await sut.RegisterAsync("verify@test.com", "Test123!", null, null, null, CancellationToken.None);
        var verify = await sut.VerifyEmailAsync("verify@test.com", email.LastVerificationCode!, CancellationToken.None);

        Assert.True(verify.Success);
        var user = await db.Users.SingleAsync(u => u.Email == "verify@test.com");
        Assert.True(user.IsEmailVerified);
        Assert.Null(user.EmailVerificationToken);
        Assert.Null(user.EmailVerificationTokenExpiresAt);
    }

    [Fact]
    public async Task VerifyEmailAsync_ExpiredCode_ReturnsFailure()
    {
        await using var db = CreateDbContext();
        db.Users.Add(new User
        {
            Email = "expired@test.com",
            PasswordHash = Hash("Test123!"),
            IsEmailVerified = false,
            EmailVerificationToken = Hash("123456"),
            EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            Role = "Admin"
        });
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = CreateAuthService(db, new FakeEmailService());
        var result = await sut.VerifyEmailAsync("expired@test.com", "123456", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("expired", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginAsync_AfterVerification_ReturnsToken()
    {
        await using var db = CreateDbContext();
        var email = new FakeEmailService { VerificationResult = true };
        var sut = CreateAuthService(db, email);

        await sut.RegisterAsync("ready@test.com", "Test123!", null, null, null, CancellationToken.None);
        await sut.VerifyEmailAsync("ready@test.com", email.LastVerificationCode!, CancellationToken.None);

        var login = await sut.LoginAsync("ready@test.com", "Test123!", CancellationToken.None);

        Assert.True(login.Success);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));
    }

    private static AuthService CreateAuthService(TestEcoTrackDbContext db, FakeEmailService email)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "test-secret-key-1234567890-abcdefghijklmnopqrstuvwxyz",
                ["JwtSettings:Issuer"] = "EcoTrackAPI",
                ["JwtSettings:Audience"] = "EcoTrackClient"
            })
            .Build();

        return new AuthService(db, config, email);
    }

    private static TestEcoTrackDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestEcoTrackDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestEcoTrackDbContext(options);
    }

    private static string Hash(string value)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }

    private sealed class FakeEmailService : IEmailService
    {
        public bool VerificationResult { get; set; } = true;
        public string? LastEmail { get; private set; }
        public string? LastVerificationCode { get; private set; }

        public Task<bool> SendVerificationEmailAsync(string email, string verificationCode, CancellationToken cancellationToken = default)
        {
            LastEmail = email;
            LastVerificationCode = verificationCode;
            return Task.FromResult(VerificationResult);
        }

        public Task<bool> SendPasswordResetEmailAsync(string email, string resetLink, CancellationToken cancellationToken = default)
            => Task.FromResult(true);
    }

    private sealed class TestEcoTrackDbContext : DbContext, IEcoTrackDbContext
    {
        public TestEcoTrackDbContext(DbContextOptions<TestEcoTrackDbContext> options) : base(options)
        {
        }

        public DbSet<Company> Companies => Set<Company>();
        public DbSet<EmissionEntry> EmissionEntries => Set<EmissionEntry>();
        public DbSet<EmissionCategory> EmissionCategories => Set<EmissionCategory>();
        public DbSet<User> Users => Set<User>();

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => base.SaveChangesAsync(cancellationToken);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmissionCategory>().Ignore("Embedding");
            modelBuilder.Entity<EmissionEntry>().Ignore("Embedding");
            base.OnModelCreating(modelBuilder);
        }
    }
}
