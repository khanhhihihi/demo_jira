using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VietNhatHospital.Data;

namespace VietNhatHospital.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiseasesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DiseasesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /api/diseases
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var conditions = await _context.Conditions
            .Select(c => new
            {
                id = c.ConditionId,
                name = c.Name,
                diseaseGroup = c.Category ?? "",
                icd10Code = c.IcdCode ?? "",
                status = c.IsActive ? "active" : "inactive",
                description = c.Description ?? ""
            })
            .ToListAsync();

        return Ok(conditions);
    }
}
