using System;

namespace VietNhatHospital.Models;

public class ErrorReport
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty; // e.g. "MB12-001"
    public string DrugName { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
    public string Reporter { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "pending"; // "pending" | "inReview" | "resolved"
    public string Priority { get; set; } = "medium"; // "high" | "medium" | "low"
    public string Description { get; set; } = string.Empty;
    public string? AdminNote { get; set; }
}
