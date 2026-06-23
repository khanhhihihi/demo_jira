using Microsoft.AspNetCore.Identity;

namespace VietNhatHospital.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    
    public string Status { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Clinical Patient Profile Fields
    public double? Height { get; set; } // in cm or meters

    public double? Weight { get; set; } // in kg

    public DateTime? BirthDate { get; set; }

    public string? Gender { get; set; } // "male", "female", "other"

    public string? Notes { get; set; }

    // Navigation properties
    public virtual ICollection<PatientCondition> PatientConditions { get; set; } = new List<PatientCondition>();
}
