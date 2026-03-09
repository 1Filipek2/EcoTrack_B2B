using EcoTrack.Application.DTOs;
using EcoTrack.Application.Features.Emissions.Queries.GetSustainabilityReport;
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
public class CompaniesController : ControllerBase
{
    private readonly IEcoTrackDbContext _context;
    private readonly ILogger<CompaniesController> _logger;
    private readonly IMediator _mediator;

    public CompaniesController(IEcoTrackDbContext context, ILogger<CompaniesController> logger, IMediator mediator)
    {
        _context = context;
        _logger = logger;
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Company>> CreateCompany([FromBody] CreateCompanyRequest request)
    {
        try
        {
            var company = new Company
            {
                Name = request.Name,
                VatNumber = request.VatNumber
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync(CancellationToken.None);

            return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, company);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Company>> GetCompany(Guid id)
    {
        var company = await _context.Companies.FindAsync(id);
        
        if (company == null)
            return NotFound();

        return Ok(company);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<Company>>> GetCompanies([FromQuery] PaginationParams pagination)
    {
        var query = _context.Companies.AsQueryable();
        
        var totalCount = await query.CountAsync();
        
        var companies = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var result = new PagedResult<Company>
        {
            Items = companies,
            TotalCount = totalCount,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };

        return Ok(result);
    }

    [HttpGet("{id}/sustainability-report")]
    public async Task<ActionResult<SustainabilityReportDto>> GetSustainabilityReport(
        Guid id,
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var report = await _mediator.Send(
                new GetSustainabilityReportQuery(id, startDate, endDate),
                cancellationToken);

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sustainability report");
            return StatusCode(500, new { error = "An error occurred generating the report." });
        }
    }
}

public class CreateCompanyRequest
{
    public string Name { get; set; } = string.Empty;
    public string VatNumber { get; set; } = string.Empty;
}


