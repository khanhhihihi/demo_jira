using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietNhatHospital.Models;

public class PatientCondition
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int ConditionId { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser? Patient { get; set; }

    [ForeignKey("ConditionId")]
    public virtual Condition? Condition { get; set; }
}
