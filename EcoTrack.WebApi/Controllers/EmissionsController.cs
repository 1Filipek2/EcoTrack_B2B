using EcoTrack.Application.DTOs;
using EcoTrack.Application.Features.Emissions.Commands.ProcessUnstructuredData;
using EcoTrack.Application.Interfaces;
using EcoTrack.Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<ActionResult<EmissionDto>> CreateEmission([FromBody] CreateEmissionRequest request)
    {
        try
        {
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

            emission.SetEmissionFactor(0.233m);

            _context.EmissionEntries.Add(emission);
            await _context.SaveChangesAsync(CancellationToken.None);

            var dto = new EmissionDto
            {
                Id = emission.Id,
                CompanyId = company.Id,
                Category = category.Name,
                Amount = emission.Amount,
                Co2Equivalent = emission.Co2Equivalent,
                ReportDate = emission.ReportDate
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
    public async Task<ActionResult<Guid>> ProcessUnstructured([FromBody] ProcessUnstructuredEmissionRequest request, CancellationToken cancellationToken)
    {
        var emissionId = await _mediator.Send(
            new ProcessUnstructuredDataCommand(request.CompanyId, request.RawText, request.ReportedDate),
            cancellationToken);

        return CreatedAtAction(nameof(GetEmission), new { id = emissionId }, emissionId);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmissionDto>> GetEmission(Guid id)
    {
        var emission = await _context.EmissionEntries
            .Include(e => e.Company)
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (emission == null)
            return NotFound();

        var dto = new EmissionDto
        {
            Id = emission.Id,
            CompanyId = emission.Company.Id,
            Category = emission.Category.Name,
            Amount = emission.Amount,
            Co2Equivalent = emission.Co2Equivalent,
            ReportDate = emission.ReportDate
        };

        return Ok(dto);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<EmissionDto>>> GetEmissions([FromQuery] PaginationParams pagination)
    {
        var query = _context.EmissionEntries
            .Include(e => e.Company)
            .Include(e => e.Category);
        
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
                ReportDate = e.ReportDate
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
}

public class CreateEmissionRequest
{
    public Guid CompanyId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset ReportedDate { get; set; }
    public string RawData { get; set; } = string.Empty;
}

public class ProcessUnstructuredEmissionRequest
{
    public Guid CompanyId { get; set; }
    public string RawText { get; set; } = string.Empty;
    public DateTimeOffset? ReportedDate { get; set; }
}
