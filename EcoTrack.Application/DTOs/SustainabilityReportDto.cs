namespace EcoTrack.Application.DTOs;

public class SustainabilityReportDto
{
    public Guid CompanyId { get; set; }
    public decimal TotalEmissions { get; set; }
    public decimal Scope1 { get; set; }
    public decimal Scope2 { get; set; }
    public decimal Scope3 { get; set; }
}