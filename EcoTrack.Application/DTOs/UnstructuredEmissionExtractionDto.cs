namespace EcoTrack.Application.DTOs;

public class UnstructuredEmissionExtractionDto
{
    public string Activity { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Guid? MatchedCategoryId { get; set; }
    public string? MatchedCategoryName { get; set; }
    public double SimilarityScore { get; set; }
}

