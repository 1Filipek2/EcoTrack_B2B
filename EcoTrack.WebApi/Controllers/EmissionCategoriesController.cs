using EcoTrack.Application.Interfaces;
using EcoTrack.Core.Entities;
using EcoTrack.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoTrack.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmissionCategoriesController : ControllerBase
{
    private readonly IEcoTrackDbContext _context;
    private readonly ILogger<EmissionCategoriesController> _logger;

    public EmissionCategoriesController(IEcoTrackDbContext context, ILogger<EmissionCategoriesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<EmissionCategory>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var category = new EmissionCategory
            {
                Name = request.Name,
                Description = request.Description,
                Scope = request.Scope
            };

            _context.EmissionCategories.Add(category);
            await _context.SaveChangesAsync(CancellationToken.None);

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmissionCategory>> GetCategory(Guid id)
    {
        var category = await _context.EmissionCategories.FindAsync(id);
        
        if (category == null)
            return NotFound();

        return Ok(category);
    }

    [HttpGet]
    public ActionResult<IEnumerable<EmissionCategory>> GetCategories()
    {
        var categories = _context.EmissionCategories.ToList();
        return Ok(categories);
    }
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EmissionScope Scope { get; set; }
}


