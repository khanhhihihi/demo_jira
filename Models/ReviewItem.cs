using System;

namespace VietNhatHospital.Models;

public class ReviewItem
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty; // e.g. "MB13-001"
    public string Type { get; set; } = string.Empty; // e.g. "Thêm thuốc", "Sửa chống chỉ định"
    public string Content { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "pending"; // "pending" | "approved" | "rejected" | "needsRevision"
    public string Reviewer { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string? RejectionNote { get; set; }
}
