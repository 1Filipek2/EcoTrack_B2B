namespace EcoTrack.Application.DTOs;

public class EmissionDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Co2Equivalent { get; set; }
    public DateTimeOffset ReportDate { get; set; }
    public string RawData { get; set; } = string.Empty;
}