namespace EcoTrack.Application.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Token, string Role, Guid? CompanyId)> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> RegisterAsync(string email, string password, Guid? companyId, CancellationToken cancellationToken = default);
    string GenerateJwtToken(Guid userId, string email, string role, Guid? companyId);
}

