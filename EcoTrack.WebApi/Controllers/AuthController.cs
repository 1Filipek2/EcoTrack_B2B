using EcoTrack.Application.DTOs;
using EcoTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoTrack.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var (success, token, role, companyId) = await _authService.LoginAsync(
                request.Email, 
                request.Password, 
                cancellationToken);

            if (!success)
                return Unauthorized(new { error = "Invalid email or password." });

            return Ok(new AuthResponse
            {
                Token = token,
                Email = request.Email,
                Role = role,
                CompanyId = companyId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { error = "An error occurred during login." });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var (success, message) = await _authService.RegisterAsync(
                request.Email, 
                request.Password, 
                request.CompanyId, 
                cancellationToken);

            if (!success)
                return BadRequest(new { error = message });

            return Ok(new { message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new { error = "An error occurred during registration." });
        }
    }

    [Authorize]
    [HttpPost("link-company")]
    public async Task<ActionResult<AuthResponse>> LinkCompany([FromBody] LinkCompanyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || Guid.TryParse(userIdClaim, out var userId) == false)
                return Unauthorized(new { error = "Invalid user token." });

            var (success, token, email, role, companyId, message) = await _authService.LinkCompanyAsync(userId, request.CompanyId, cancellationToken);
            if (!success)
                return BadRequest(new { error = message });

            return Ok(new AuthResponse
            {
                Token = token,
                Email = email,
                Role = role,
                CompanyId = companyId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking company to user");
            return StatusCode(500, new { error = "An error occurred while linking company." });
        }
    }
}
