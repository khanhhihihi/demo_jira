//- Thực hiện bởi Thành viên A(Tiền)
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VietNhatHospital.Data;
using VietNhatHospital.Models;

namespace VietNhatHospital.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HistoryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
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

    // GET /api/history?q={keyword}&type={type}&date={date}&page={page}&pageSize={pageSize}
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? q,
        [FromQuery] string? type,
        [FromQuery] string? date,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        IQueryable<SearchHistoryItem> query = _context.SearchHistories;

        // Segregate search history: Non-admins can only see their own history
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Ok(new
            {
                items = new List<object>(),
                total = 0,
                page = page,
                pageSize = pageSize
            });
        }

        var user = await _userManager.FindByIdAsync(userId);
        var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

        if (!isAdmin)
        {
            query = query.Where(h => h.UserId == userId);
        }

        // Apply keyword filter
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim().ToLower();
            query = query.Where(h => h.Keyword.ToLower().Contains(q) || h.Result.ToLower().Contains(q));
        }

        // Apply type filter
        if (!string.IsNullOrWhiteSpace(type) && type != "all")
        {
            query = query.Where(h => h.Type == type);
        }

        // Apply date filter
        if (!string.IsNullOrWhiteSpace(date))
        {
            if (DateTime.TryParse(date, out DateTime parsedDate))
            {
                var startOfDay = parsedDate.Date;
                var endOfDay = startOfDay.AddDays(1);
                query = query.Where(h => h.Timestamp >= startOfDay && h.Timestamp < endOfDay);
            }
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(h => h.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(h => new
            {
                id = h.Id,
                timestamp = h.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                type = h.Type,
                keyword = h.Keyword,
                result = h.Result,
                detailsUrl = h.DetailsUrl ?? ""
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
}
