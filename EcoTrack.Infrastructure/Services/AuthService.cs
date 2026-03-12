using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EcoTrack.Application.Interfaces;
using EcoTrack.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EcoTrack.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IEcoTrackDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthService(IEcoTrackDbContext context, IConfiguration configuration, IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<(bool Success, string Token, string Role, Guid? CompanyId)> LoginAsync(
        string email, 
        string password, 
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user == null)
            return (false, string.Empty, string.Empty, null);

        if (!user.IsEmailVerified)
            return (false, string.Empty, string.Empty, null);

        if (!VerifyPassword(password, user.PasswordHash))
            return (false, string.Empty, string.Empty, null);

        var token = GenerateJwtToken(user.Id, user.Email, user.Role, user.CompanyId);

        return (true, token, user.Role, user.CompanyId);
    }

    public async Task<(bool Success, string Message)> RegisterAsync(
        string email, 
        string password, 
        Guid? companyId, 
        string? companyName, 
        string? vatNumber, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (existingUser != null)
                return (false, "User with this email already exists.");

            Guid? finalCompanyId = null;
            if (!string.IsNullOrWhiteSpace(companyName) && !string.IsNullOrWhiteSpace(vatNumber))
            {
                var newCompany = new Company { Name = companyName, VatNumber = vatNumber };
                _context.Companies.Add(newCompany);
                await _context.SaveChangesAsync(cancellationToken);
                finalCompanyId = newCompany.Id;
            }
            else if (companyId.HasValue)
            {
                var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId.Value, cancellationToken);
                if (!companyExists)
                    return (false, "Provided CompanyId does not exist.");
                finalCompanyId = companyId;
            }

            var verificationCode = GenerateVerificationCode();

            var user = new User
            {
                Email = email,
                PasswordHash = HashPassword(password),
                Role = finalCompanyId.HasValue ? "CompanyUser" : "Admin",
                CompanyId = finalCompanyId,
                IsEmailVerified = false,
                EmailVerificationToken = HashPassword(verificationCode),
                EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            var sent = await _emailService.SendVerificationEmailAsync(email, verificationCode, cancellationToken);

            if (!sent)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync(cancellationToken);
                return (false, "Unable to send verification email. Please ensure SendGrid is properly configured and try again.");
            }

            return (true, "User registered successfully. Please check your email for the verification code.");
        }
        catch (Exception ex)
        {
            return (false, $"Registration failed: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> VerifyEmailAsync(string email, string verificationCode, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user == null)
            return (false, "User not found.");

        if (user.IsEmailVerified)
            return (false, "Email already verified.");

        if (user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
            return (false, "Verification code expired.");

        if (!VerifyPassword(verificationCode, user.EmailVerificationToken))
            return (false, "Invalid verification code.");

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiresAt = null;

        await _context.SaveChangesAsync(cancellationToken);

        return (true, "Email verified successfully.");
    }

    public async Task<(bool Success, string Token, string Email, string Role, Guid? CompanyId, string Message)> LinkCompanyAsync(
        Guid userId,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
            return (false, string.Empty, string.Empty, string.Empty, null, "User not found.");

        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken);
        if (company == null)
            return (false, string.Empty, string.Empty, string.Empty, null, "Company not found.");

        user.CompanyId = companyId;
        if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase) == false)
            user.Role = "CompanyUser";

        await _context.SaveChangesAsync(cancellationToken);

        var token = GenerateJwtToken(user.Id, user.Email, user.Role, user.CompanyId);
        return (true, token, user.Email, user.Role, user.CompanyId, "Company linked successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
            return (false, "User not found.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
        return (true, "Account deleted successfully.");
    }

    public async Task<(bool Success, string Message)> ResendVerificationCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user == null)
            return (false, "User not found.");
        if (user.IsEmailVerified)
            return (false, "Email already verified.");
        var now = DateTime.UtcNow;
        if (user.LastVerificationCodeSentAt.HasValue && (now - user.LastVerificationCodeSentAt.Value).TotalMinutes < 5)
            return (false, $"You can resend the code only every 5 minutes. Last sent: {user.LastVerificationCodeSentAt.Value:HH:mm:ss} UTC");
        var verificationCode = GenerateVerificationCode();
        user.EmailVerificationToken = HashPassword(verificationCode);
        user.EmailVerificationTokenExpiresAt = now.AddHours(24);
        user.LastVerificationCodeSentAt = now;
        await _context.SaveChangesAsync(cancellationToken);
        var sent = await _emailService.SendVerificationEmailAsync(email, verificationCode, cancellationToken);
        if (!sent)
            return (false, "Unable to send verification email. Please try again later.");
        return (true, "Verification code resent successfully. Please check your email.");
    }

    public string GenerateJwtToken(Guid userId, string email, string role, Guid? companyId)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-min-32-chars-long-for-HS256";
        var issuer = jwtSettings["Issuer"] ?? "EcoTrackAPI";
        var audience = jwtSettings["Audience"] ?? "EcoTrackClient";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };

        if (companyId.HasValue)
            claims.Add(new Claim("CompanyId", companyId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var hashToVerify = HashPassword(password);
        return hashToVerify == storedHash;
    }

    private static string GenerateVerificationCode()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }
}
