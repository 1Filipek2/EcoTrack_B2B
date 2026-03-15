using EcoTrack.Application.DTOs;
using EcoTrack.Application.Features.Emissions.Commands.ProcessUnstructuredData;
using EcoTrack.Application.Interfaces;
using EcoTrack.Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace EcoTrack.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmissionsController : ControllerBase
{
    private readonly IEcoTrackDbContext _context;
    private readonly ILogger<EmissionsController> _logger;
    private readonly IMediator _mediator;

    public EmissionsController(IEcoTrackDbContext context, ILogger<EmissionsController> logger, IMediator mediator)
    {
        _context = context;
        _logger = logger;
        _mediator = mediator;
    }
    
        [HttpPost]
        [Authorize(Roles = "CompanyUser")]
        public async Task<ActionResult<EmissionDto>> CreateEmission([FromBody] CreateEmissionRequest request)
        {
            try
            {
                var currentCompanyId = GetCurrentCompanyId();
                if (!currentCompanyId.HasValue)
                    return BadRequest(new { error = "Your account is not linked to a company." });

                if (request.CompanyId != currentCompanyId.Value)
                    return Forbid();

                var company = await _context.Companies.FindAsync(request.CompanyId);
                if (company == null)
                    return BadRequest(new { error = $"Company with ID {request.CompanyId} not found" });

                var category = await _context.EmissionCategories.FindAsync(request.CategoryId);
                if (category == null)
                    return BadRequest(new { error = $"Category with ID {request.CategoryId} not found" });

                var emission = new EmissionEntry
                {
                    Amount = request.Amount,
                    Company = company,
                    Category = category,
                    ReportDate = request.ReportedDate,
                    RawData = request.RawData
                };
                
                emission.Co2Equivalent = emission.Amount * 0.233m;

                _context.EmissionEntries.Add(emission);
                await _context.SaveChangesAsync(CancellationToken.None);

                var dto = new EmissionDto
                {
                    Id = emission.Id,
                    CompanyId = company.Id,
                    Category = category.Name,
                    Amount = emission.Amount,
                    Co2Equivalent = emission.Co2Equivalent,
                    ReportDate = emission.ReportDate,
                    RawData = emission.RawData
                };

                return CreatedAtAction(nameof(GetEmission), new { id = emission.Id }, dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating emission");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("process-unstructured")]
        [Authorize(Roles = "CompanyUser")]
        public async Task<ActionResult<Guid>> ProcessUnstructured([FromBody] ProcessUnstructuredEmissionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var currentCompanyId = GetCurrentCompanyId();
                if (!currentCompanyId.HasValue)
                    return BadRequest(new { error = "Your account is not linked to a company." });

                if (request.CompanyId != currentCompanyId.Value)
                    return Forbid();

                var emissionId = await _mediator.Send(
                    new ProcessUnstructuredDataCommand(request.CompanyId, request.RawText, request.ReportedDate),
                    cancellationToken);

                return CreatedAtAction(nameof(GetEmission), new { id = emissionId }, emissionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing unstructured emission");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmissionDto>> GetEmission(Guid id)
        {
            try
            {
                var currentCompanyId = GetCurrentCompanyId();
                if (!currentCompanyId.HasValue)
                    return BadRequest(new { error = "Your account is not linked to a company." });

                var emission = await _context.EmissionEntries
                    .Include(e => e.Company)
                    .Include(e => e.Category)
                    .FirstOrDefaultAsync(e => e.Id == id && e.Company.Id == currentCompanyId.Value);

                if (emission == null)
                    return NotFound();

                var dto = new EmissionDto
                {
                    Id = emission.Id,
                    CompanyId = emission.Company.Id,
                    Category = emission.Category.Name,
                    Amount = emission.Amount,
                    Co2Equivalent = emission.Co2Equivalent,
                    ReportDate = emission.ReportDate,
                    RawData = emission.RawData
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting emission");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateEmission(Guid id, [FromBody] UpdateEmissionRequest request)
        {
            try
            {
                var currentCompanyId = GetCurrentCompanyId();
                if (!currentCompanyId.HasValue)
                    return BadRequest(new { error = "Your account is not linked to a company." });

                var emission = await _context.EmissionEntries
                    .Include(e => e.Company)
                    .FirstOrDefaultAsync(e => e.Id == id && e.Company.Id == currentCompanyId.Value && !e.IsDeleted);

                if (emission == null)
                    return NotFound();

                emission.Amount = request.Amount;
                emission.ReportDate = request.ReportedDate;
                emission.RawData = request.RawData;
                emission.LastModifiedAt = DateTimeOffset.UtcNow;
                emission.LastModifiedBy = User.Identity?.Name ?? "unknown";
                await _context.SaveChangesAsync(CancellationToken.None);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating emission");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "CompanyUser")]
        public async Task<ActionResult> DeleteEmission(Guid id)
        {
            try
            {
                var currentCompanyId = GetCurrentCompanyId();
                if (!currentCompanyId.HasValue)
                    return BadRequest(new { error = "Your account is not linked to a company." });

                var emission = await _context.EmissionEntries
                    .Include(e => e.Company)
                    .FirstOrDefaultAsync(e => e.Id == id && e.Company.Id == currentCompanyId.Value && !e.IsDeleted);

                if (emission == null)
                    return NotFound();

                emission.IsDeleted = true;
                emission.DeletedAt = DateTimeOffset.UtcNow;
                emission.DeletedBy = User.Identity?.Name ?? "unknown";
                await _context.SaveChangesAsync(CancellationToken.None);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting emission");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<EmissionDto>>> GetEmissions([FromQuery] PaginationParams pagination)
        {
            try
            {
                var currentCompanyId = GetCurrentCompanyId();
                if (!currentCompanyId.HasValue)
                {
                    return Ok(new PagedResult<EmissionDto>
                    {
                        Items = new List<EmissionDto>(),
                        TotalCount = 0,
                        PageNumber = pagination.PageNumber,
                        PageSize = pagination.PageSize
                    });
                }

                var query = _context.EmissionEntries
                    .Include(e => e.Company)
                    .Include(e => e.Category)
                    .Where(e => e.Company.Id == currentCompanyId.Value && !e.IsDeleted)
                    .OrderByDescending(e => e.ReportDate);
        
                var totalCount = await query.CountAsync();
        
                var emissions = await query
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .Select(e => new EmissionDto
                    {
                        Id = e.Id,
                        CompanyId = e.Company.Id,
                        Category = e.Category.Name,
                        Amount = e.Amount,
                        Co2Equivalent = e.Co2Equivalent,
                        ReportDate = e.ReportDate,
                        RawData = e.RawData
                    })
                    .ToListAsync();

                var result = new PagedResult<EmissionDto>
                {
                    Items = emissions,
                    TotalCount = totalCount,
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting emissions");
                return BadRequest(new { error = ex.Message });
            }
        }

    private Guid? GetCurrentCompanyId()
    {
        var companyClaim = User.FindFirstValue("CompanyId");
        return Guid.TryParse(companyClaim, out var companyId) ? companyId : null;
    }
}

public class CreateEmissionRequest
{
    [Required]
    public Guid CompanyId { get; set; }
    [Required]
    public Guid CategoryId { get; set; }
    [Required]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Amount must be positive.")]
    public decimal Amount { get; set; }
    [Required]
    public DateTimeOffset ReportedDate { get; set; }
    [StringLength(10000)]
    public string RawData { get; set; } = string.Empty;
}

public class ProcessUnstructuredEmissionRequest
{
    [Required]
    public Guid CompanyId { get; set; }
    [Required]
    [StringLength(10000)]
    public string RawText { get; set; } = string.Empty;
    public DateTimeOffset? ReportedDate { get; set; }
}

public class UpdateEmissionRequest
{
    [Required]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Amount must be positive.")]
    public decimal Amount { get; set; }
    [Required]
    public DateTimeOffset ReportedDate { get; set; }
    [StringLength(10000)]
    public string RawData { get; set; } = string.Empty;
}
