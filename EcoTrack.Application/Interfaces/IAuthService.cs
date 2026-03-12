namespace EcoTrack.Application.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Token, string Role, Guid? CompanyId)> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> RegisterAsync(string email, string password, Guid? companyId, string? companyName, string? vatNumber, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> VerifyEmailAsync(string email, string verificationCode, CancellationToken cancellationToken = default);
    Task<(bool Success, string Token, string Email, string Role, Guid? CompanyId, string Message)> LinkCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> DeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default);
    string GenerateJwtToken(Guid userId, string email, string role, Guid? companyId);
    Task<(bool Success, string Message)> ResendVerificationCodeAsync(string email, CancellationToken cancellationToken = default);
}
