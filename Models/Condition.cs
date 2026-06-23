using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietNhatHospital.Models;

public class Condition
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ConditionId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Category { get; set; }

    public string? Description { get; set; }

    public string? IcdCode { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<DrugContraindication> Contraindications { get; set; } = new List<DrugContraindication>();
    
    public virtual ICollection<PatientCondition> PatientConditions { get; set; } = new List<PatientCondition>();
}
