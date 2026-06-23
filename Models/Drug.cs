//- Thực hiện bởi Thành viên A(Tiền)
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietNhatHospital.Models;

public class Drug
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int DrugId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? ActiveIngredient { get; set; }

    public string? DrugGroup { get; set; }

    public string? Description { get; set; }

    public string? SideEffects { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<DrugContraindication> Contraindications { get; set; } = new List<DrugContraindication>();
}
