using EcoTrack.Core.Common;

namespace EcoTrack.Core.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "CompanyUser"; // Admin, CompanyUser
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    
    // Email verification
    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }
}
