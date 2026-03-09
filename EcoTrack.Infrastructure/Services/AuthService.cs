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

    public AuthService(IEcoTrackDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
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

        if (!VerifyPassword(password, user.PasswordHash))
            return (false, string.Empty, string.Empty, null);

        var token = GenerateJwtToken(user.Id, user.Email, user.Role, user.CompanyId);

        return (true, token, user.Role, user.CompanyId);
    }

    public async Task<(bool Success, string Message)> RegisterAsync(
        string email, 
        string password, 
        Guid? companyId, 
        CancellationToken cancellationToken = default)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (existingUser != null)
            return (false, "User with this email already exists.");

        if (companyId.HasValue)
        {
            var companyExists = await _context.Companies
                .AnyAsync(c => c.Id == companyId.Value, cancellationToken);

            if (!companyExists)
                return (false, "Company not found.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = HashPassword(password),
            Role = companyId.HasValue ? "CompanyUser" : "Admin",
            CompanyId = companyId
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return (true, "User registered successfully.");
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
}

