// Task 1: Bộ lọc chống chỉ định theo bệnh nền - Nhóm trưởng phụ trách
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VietNhatHospital.Data;
using VietNhatHospital.Models;

namespace VietNhatHospital.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SearchController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string? GetCurrentUserId()
    {
        string? authHeader = Request.Headers["Authorization"];
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer user-id:"))
        {
            return authHeader.Substring("Bearer user-id:".Length).Trim();
        }
        return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    }

    // GET /api/search/autocomplete?term={query}
    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete([FromQuery] string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return Ok(new List<object>());
        }

        term = term.Trim().ToLower();

        // 1. Search matching conditions (diseases)
        var conditions = await _context.Conditions
            .Where(c => c.IsActive && c.Name.ToLower().Contains(term))
            .Select(c => new
            {
                id = c.ConditionId,
                name = c.Name,
                type = "disease"
            })
            .ToListAsync();

        // 2. Search matching drugs
        var drugs = await _context.Drugs
            .Where(d => d.IsActive && (d.Name.ToLower().Contains(term) || (d.ActiveIngredient != null && d.ActiveIngredient.ToLower().Contains(term))))
            .Select(d => new
            {
                id = d.DrugId,
                name = d.Name,
                type = "drug"
            })
            .ToListAsync();

        // Combine both lists
        var results = conditions.Cast<object>().Concat(drugs.Cast<object>()).ToList();

        return Ok(results);
    }

    // GET /api/search/contraindications?diseaseId={id}
    [HttpGet("contraindications")]
    public async Task<IActionResult> GetContraindications([FromQuery] int diseaseId)
    {
        var diseaseExists = await _context.Conditions.AnyAsync(c => c.ConditionId == diseaseId);
        if (!diseaseExists)
        {
            return NotFound(new { message = "Không tìm thấy bệnh lý." });
        }

        var contraindications = await _context.DrugContraindications
            .Include(dc => dc.Drug)
            .Include(dc => dc.Condition)
            .Where(dc => dc.ConditionId == diseaseId)
            .Select(dc => new
            {
                drugName = dc.Drug != null ? dc.Drug.Name : "",
                warningLevel = dc.Severity.ToLower() == "critical" ? "Nguy hiểm" : "Cảnh báo",
                activeIngredient = dc.Drug != null ? dc.Drug.ActiveIngredient : "",
                description = dc.Reason
            })
            .ToListAsync();

        // Log into search history
        try
        {
            var disease = await _context.Conditions.FindAsync(diseaseId);
            if (disease != null)
            {
                _context.SearchHistories.Add(new SearchHistoryItem
                {
                    Timestamp = DateTime.UtcNow,
                    Type = "Bệnh lý",
                    Keyword = disease.Name,
                    Result = $"Tìm thấy {contraindications.Count} chống chỉ định",
                    UserId = GetCurrentUserId()
                });
                await _context.SaveChangesAsync();
            }
        }
        catch { }

        return Ok(contraindications);
    }
}
