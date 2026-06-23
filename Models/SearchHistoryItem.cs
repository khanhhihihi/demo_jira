//Thực hiện bởi Thành viên A(Tiền)
using System;

namespace VietNhatHospital.Models;

public class SearchHistoryItem
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Type { get; set; } = string.Empty; // "Thuốc" | "Tương tác" | "Hồ sơ"
    public string Keyword { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? DetailsUrl { get; set; }
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
