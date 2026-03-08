using EcoTrack.Application.Interfaces;
using EcoTrack.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EcoTrack.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly IEcoTrackDbContext _context;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(IEcoTrackDbContext context, ILogger<CompaniesController> logger)
    {
        _context = context;
        _logger = logger;
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
    public ActionResult<IEnumerable<Company>> GetCompanies()
    {
        var companies = _context.Companies.ToList();
        return Ok(companies);
    }
}

public class CreateCompanyRequest
{
    public string Name { get; set; } = string.Empty;
    public string VatNumber { get; set; } = string.Empty;
}


