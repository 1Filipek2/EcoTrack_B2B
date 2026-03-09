namespace EcoTrack.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendVerificationEmailAsync(string email, string verificationCode, CancellationToken cancellationToken = default);
    Task<bool> SendPasswordResetEmailAsync(string email, string resetLink, CancellationToken cancellationToken = default);
}

