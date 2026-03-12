using EcoTrack.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EcoTrack.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendVerificationEmailAsync(string email, string verificationCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var sendGridApiKey = _configuration["SendGrid:ApiKey"];
            var fromEmail = _configuration["SendGrid:FromEmail"];
            var fromName = _configuration["SendGrid:FromName"] ?? "EcoTrack";

            if (string.IsNullOrEmpty(sendGridApiKey))
            {
                _logger.LogWarning("SendGrid API Key not configured. Skipping email send for {Email}", email);
                return false;
            }

            if (string.IsNullOrEmpty(fromEmail))
            {
                _logger.LogError("SendGrid FromEmail not configured. Cannot send email to {Email}", email);
                return false;
            }

            var client = new SendGridClient(sendGridApiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(email);
            var subject = "EcoTrack - Email Verification";
            
            var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #10b981 0%, #059669 100%); padding: 30px; border-radius: 10px 10px 0 0; text-align: center;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>Welcome to EcoTrack!</h1>
    </div>
    <div style='background: #f9fafb; padding: 30px; border-radius: 0 0 10px 10px;'>
        <p style='font-size: 16px; color: #374151; margin-bottom: 20px;'>
            Thank you for registering with EcoTrack. To complete your registration, please verify your email address by entering the code below:
        </p>
        <div style='background: white; border: 2px solid #10b981; border-radius: 8px; padding: 20px; text-align: center; margin: 30px 0;'>
            <div style='color: #10b981; font-size: 36px; font-weight: bold; letter-spacing: 8px; font-family: monospace;'>
                {verificationCode}
            </div>
        </div>
        <p style='font-size: 14px; color: #6b7280; margin-top: 20px;'>
            This code will expire in <strong>24 hours</strong>.
        </p>
        <p style='font-size: 14px; color: #6b7280; margin-top: 20px;'>
            If you didn't create this account, please ignore this email.
        </p>
        <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 30px 0;'>
        <p style='font-size: 12px; color: #9ca3af; text-align: center;'>
            © 2026 EcoTrack. All rights reserved.
        </p>
    </div>
</body>
</html>";

            var plainTextContent = $@"
Welcome to EcoTrack!

Thank you for registering. Please verify your email by entering the code below:

{verificationCode}

This code expires in 24 hours.

If you didn't create this account, please ignore this email.

© 2026 EcoTrack. All rights reserved.
";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Verification email sent successfully to {Email}", email);
                return true;
            }
            else
            {
                var responseBody = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogError("SendGrid API error: {StatusCode} - {Body}", response.StatusCode, responseBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification email to {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetLink, CancellationToken cancellationToken = default)
    {
        try
        {
            var sendGridApiKey = _configuration["SendGrid:ApiKey"];
            var fromEmail = _configuration["SendGrid:FromEmail"];
            var fromName = _configuration["SendGrid:FromName"] ?? "EcoTrack";

            if (string.IsNullOrEmpty(sendGridApiKey))
            {
                _logger.LogWarning("SendGrid API Key not configured. Skipping email send for {Email}", email);
                return false;
            }

            if (string.IsNullOrEmpty(fromEmail))
            {
                _logger.LogError("SendGrid FromEmail not configured. Cannot send email to {Email}", email);
                return false;
            }

            var client = new SendGridClient(sendGridApiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(email);
            var subject = "EcoTrack - Password Reset";
            
            var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #10b981 0%, #059669 100%); padding: 30px; border-radius: 10px 10px 0 0; text-align: center;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>Password Reset Request</h1>
    </div>
    <div style='background: #f9fafb; padding: 30px; border-radius: 0 0 10px 10px;'>
        <p style='font-size: 16px; color: #374151; margin-bottom: 20px;'>
            We received a request to reset your password for your EcoTrack account. Click the button below to reset it:
        </p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{resetLink}' style='display: inline-block; background: #10b981; color: white; padding: 14px 32px; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 16px;'>
                Reset Password
            </a>
        </div>
        <p style='font-size: 14px; color: #6b7280; margin-top: 20px;'>
            This link will expire in <strong>1 hour</strong>.
        </p>
        <p style='font-size: 14px; color: #6b7280; margin-top: 20px;'>
            If you didn't request a password reset, please ignore this email. Your password will remain unchanged.
        </p>
        <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 30px 0;'>
        <p style='font-size: 12px; color: #9ca3af;'>
            If the button doesn't work, copy and paste this link into your browser:<br>
            <a href='{resetLink}' style='color: #10b981; word-break: break-all;'>{resetLink}</a>
        </p>
        <p style='font-size: 12px; color: #9ca3af; text-align: center; margin-top: 30px;'>
            © 2026 EcoTrack. All rights reserved.
        </p>
    </div>
</body>
</html>";

            var plainTextContent = $@"
Password Reset Request

We received a request to reset your password for your EcoTrack account.

Click the link below to reset it:
{resetLink}

This link expires in 1 hour.

If you didn't request a password reset, please ignore this email.

© 2026 EcoTrack. All rights reserved.
";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Password reset email sent successfully to {Email}", email);
                return true;
            }
            else
            {
                var responseBody = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogError("SendGrid API error: {StatusCode} - {Body}", response.StatusCode, responseBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to {Email}", email);
            return false;
        }
    }
}
