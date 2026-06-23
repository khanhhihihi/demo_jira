
// Task 2 & 6: Tương tác thuốc và Lịch sử tra cứu - Thực hiện bởi Thành viên A(Tiền)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VietNhatHospital.Data;
using VietNhatHospital.Models;

namespace VietNhatHospital.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DrugsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DrugsController(ApplicationDbContext context)
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

    // 1. GET /api/drugs/search?q={keyword}
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(new List<object>());
        }

        q = q.Trim().ToLower();

        var drugs = await _context.Drugs
            .Where(d => d.Name.ToLower().Contains(q) || (d.ActiveIngredient != null && d.ActiveIngredient.ToLower().Contains(q)))
            .Include(d => d.Contraindications)
                .ThenInclude(dc => dc.Condition)
            .ToListAsync();

        var results = drugs.Select(d =>
        {
            var worstSeverity = "safe";
            if (d.Contraindications.Any(dc => dc.Severity.ToLower() == "critical"))
            {
                worstSeverity = "critical";
            }
            else if (d.Contraindications.Any(dc => dc.Severity.ToLower() == "warning"))
            {
                worstSeverity = "warning";
            }

            return new
            {
                id = d.DrugId,
                drugName = d.Name,
                activeIngredient = d.ActiveIngredient ?? "",
                severity = worstSeverity,
                contraindications = d.Contraindications.Select(dc => dc.Condition?.Name ?? "").ToList()
            };
        }).ToList();

        try
        {
            _context.SearchHistories.Add(new SearchHistoryItem
            {
                Timestamp = DateTime.UtcNow,
                Type = "Thuốc",
                Keyword = q,
                Result = $"Tìm thấy {results.Count} kết quả",
                DetailsUrl = results.Count == 1 ? $"/drugs/{results[0].id}" : "",
                UserId = GetCurrentUserId()
            });
            await _context.SaveChangesAsync();
        }
        catch { }

        return Ok(results);
    }

    // 2. GET /api/drugs?condition={diseaseId}&...
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? condition, 
        [FromQuery] string? group, 
        [FromQuery] string? severity, 
        [FromQuery] string? q, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        IQueryable<DrugContraindication> query = _context.DrugContraindications
            .Include(dc => dc.Drug)
            .Include(dc => dc.Condition);

        if (!string.IsNullOrWhiteSpace(condition) && condition != "all")
        {
            if (int.TryParse(condition, out int condId))
            {
                query = query.Where(dc => dc.ConditionId == condId);
            }
            else
            {
                query = query.Where(dc => dc.Condition != null && dc.Condition.Name.ToLower().Contains(condition.ToLower()));
            }
        }

        if (!string.IsNullOrWhiteSpace(group) && group != "all")
        {
            query = query.Where(dc => dc.Drug != null && dc.Drug.DrugGroup == group);
        }

        if (!string.IsNullOrWhiteSpace(severity) && severity != "all")
        {
            query = query.Where(dc => dc.Severity.ToLower() == severity.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.ToLower();
            query = query.Where(dc => 
                (dc.Drug != null && dc.Drug.Name.ToLower().Contains(q)) || 
                (dc.Condition != null && dc.Condition.Name.ToLower().Contains(q))
            );
        }

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(dc => new
            {
                id = dc.DrugId,
                drugName = dc.Drug != null ? dc.Drug.Name : "",
                activeIngredient = dc.Drug != null ? dc.Drug.ActiveIngredient : "",
                drugGroup = dc.Drug != null ? dc.Drug.DrugGroup : "",
                severity = dc.Severity.ToLower(),
                condition = dc.Condition != null ? dc.Condition.Name : "",
                reason = dc.Reason,
                alternative = dc.Alternative ?? "",
                alternativeNote = ""
            })
            .ToListAsync();

        return Ok(new
        {
            items = items,
            total = total,
            page = page,
            pageSize = pageSize
        });
    }

    // 3. GET /api/drugs/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var drug = await _context.Drugs
            .Include(d => d.Contraindications)
                .ThenInclude(dc => dc.Condition)
            .FirstOrDefaultAsync(d => d.DrugId == id);

        if (drug == null)
        {
            return NotFound(new { message = "Không tìm thấy thuốc." });
        }

        return Ok(new
        {
            id = drug.DrugId,
            name = drug.Name,
            activeIngredient = drug.ActiveIngredient ?? "",
            drugGroup = drug.DrugGroup ?? "",
            status = drug.IsActive ? "active" : "inactive",
            description = drug.Description ?? "",
            sideEffects = drug.SideEffects ?? "",
            contraindications = drug.Contraindications.Select(dc => new
            {
                id = dc.Id,
                drugId = dc.DrugId,
                disease = dc.Condition?.Name ?? "",
                diseaseId = dc.ConditionId,
                severity = dc.Severity.ToLower(),
                reason = dc.Reason,
                alternative = dc.Alternative ?? ""
            }).ToList()
        });
    }

    // 4. POST /api/check-interaction (Root route mapping)
    [HttpPost("/api/check-interaction")]
    public async Task<IActionResult> CheckInteraction([FromBody] CheckInteractionRequest request)
    {
        if (request == null || request.DrugNames == null || request.DrugNames.Count < 2)
        {
            return Ok(new
            {
                totalScore = 0,
                interactions = new List<object>()
            });
        }

        // Resolve names to Drug entities
        var drugNamesNormalized = request.DrugNames.Select(n => n.Trim().ToLower()).ToList();
        var drugs = await _context.Drugs
            .Where(d => drugNamesNormalized.Contains(d.Name.ToLower()))
            .ToListAsync();

        var drugIds = drugs.Select(d => d.DrugId).ToList();
        var interactionsList = new List<object>();
        int totalScore = 0;

        for (int i = 0; i < drugIds.Count; i++)
        {
            for (int j = i + 1; j < drugIds.Count; j++)
            {
                int id1 = drugIds[i];
                int id2 = drugIds[j];

                var interaction = await _context.DrugInteractions
                    .Include(di => di.Drug1)
                    .Include(di => di.Drug2)
                    .FirstOrDefaultAsync(di =>
                        (di.DrugId1 == id1 && di.DrugId2 == id2) ||
                        (di.DrugId1 == id2 && di.DrugId2 == id1)
                    );

                if (interaction != null)
                {
                    var severity = interaction.Level.ToLower();
                    interactionsList.Add(new
                    {
                        id = interaction.Id,
                        drug1 = interaction.Drug1?.Name ?? "",
                        drug2 = interaction.Drug2?.Name ?? "",
                        severity = severity,
                        reason = interaction.Description,
                        recommendation = interaction.Recommendation ?? ""
                    });

                    if (severity == "critical")
                    {
                        totalScore += 3;
                    }
                    else if (severity == "warning")
                    {
                        totalScore += 1;
                    }
                }
            }
        }
        try
        {
            _context.SearchHistories.Add(new SearchHistoryItem
            {
                Timestamp = DateTime.UtcNow,
                Type = "Tương tác",
                Keyword = string.Join(", ", request.DrugNames),
                Result = $"Phát hiện {interactionsList.Count} tương tác, tổng điểm {totalScore}",
                UserId = GetCurrentUserId()
            });
            await _context.SaveChangesAsync();
        }
        catch { }

        return Ok(new
        {
            totalScore = totalScore,
            interactions = interactionsList
        });
    }
}

public class CheckInteractionRequest
{
    public List<string> DrugNames { get; set; } = new();
}
