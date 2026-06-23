using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietNhatHospital.Models;

public class DrugContraindication
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int DrugId { get; set; }

    [Required]
    public int ConditionId { get; set; }

    [Required]
    [StringLength(20)]
    public string Severity { get; set; } = string.Empty; // "critical", "warning", "safe"

    [Required]
    public string Reason { get; set; } = string.Empty;

    public string? Alternative { get; set; }

    public string? Reference { get; set; }

    // Navigation properties
    [ForeignKey("DrugId")]
    public virtual Drug? Drug { get; set; }

    [ForeignKey("ConditionId")]
    public virtual Condition? Condition { get; set; }
}
