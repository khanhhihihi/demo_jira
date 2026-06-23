using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietNhatHospital.Models;

public class DrugInteraction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int DrugId1 { get; set; }

    [Required]
    public int DrugId2 { get; set; }

    [Required]
    [StringLength(20)]
    public string Level { get; set; } = string.Empty; // "critical", "warning", "safe"

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? Recommendation { get; set; }

    // Navigation properties
    [ForeignKey("DrugId1")]
    public virtual Drug? Drug1 { get; set; }

    [ForeignKey("DrugId2")]
    public virtual Drug? Drug2 { get; set; }
}
