using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VietNhatHospital.Data;
using VietNhatHospital.Models;

namespace VietNhatHospital.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PatientsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        // 1. Try to get current user via Authorization header token
        string? authHeader = Request.Headers["Authorization"];
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer user-id:"))
        {
            var tokenUserId = authHeader.Substring("Bearer user-id:".Length).Trim();
            var user = await _context.Users
                .Include(u => u.PatientConditions)
                    .ThenInclude(pc => pc.Condition)
                .FirstOrDefaultAsync(u => u.Id == tokenUserId);
            if (user != null) return user;
        }

        // 2. Try to get current user via Identity Claims
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            return await _context.Users
                .Include(u => u.PatientConditions)
                    .ThenInclude(pc => pc.Condition)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        return null;
    }

    // 1. GET /api/patients/me
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy hồ sơ người dùng." });
        }

        // Calculate BMI
        double bmi = 0;
        if (user.Height > 0 && user.Weight > 0)
        {
            double heightInMeters = user.Height.Value >= 3 ? user.Height.Value / 100 : user.Height.Value;
            bmi = user.Weight.Value / (heightInMeters * heightInMeters);
        }

        return Ok(new
        {
            id = user.Id,
            fullName = user.FullName ?? "",
            dateOfBirth = user.BirthDate?.ToString("yyyy-MM-dd") ?? "",
            gender = user.Gender ?? "other",
            weight = user.Weight ?? 0.0,
            height = user.Height ?? 0.0,
            notes = user.Notes ?? "",
            conditions = user.PatientConditions.Select(pc => pc.Condition?.Name ?? "").ToList(),
            conditionIds = user.PatientConditions.Select(pc => pc.ConditionId).ToList(),
            medications = new List<string>(), // Extended feature mock
            bmi = bmi
        });
    }

    // 2. PUT /api/patients/me
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy hồ sơ người dùng." });
        }

        // Update basic fields
        user.FullName = request.FullName;
        user.Notes = request.Notes;
        user.Gender = request.Gender;
        user.Weight = request.Weight;
        user.Height = request.Height;

        if (DateTime.TryParse(request.DateOfBirth, out DateTime dob))
        {
            user.BirthDate = dob;
        }

        // Update patient conditions (many-to-many)
        // Remove existing links
        _context.PatientConditions.RemoveRange(user.PatientConditions);
        await _context.SaveChangesAsync();

        // Add new links
        if (request.Conditions != null && request.Conditions.Count > 0)
        {
            foreach (var condName in request.Conditions)
            {
                var cond = await _context.Conditions.FirstOrDefaultAsync(c => c.Name.ToLower() == condName.ToLower());
                if (cond != null)
                {
                    _context.PatientConditions.Add(new PatientCondition
                    {
                        UserId = user.Id,
                        ConditionId = cond.ConditionId
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        try
        {
            _context.SearchHistories.Add(new SearchHistoryItem
            {
                Timestamp = DateTime.UtcNow,
                Type = "Hồ sơ",
                Keyword = "Cập nhật hồ sơ bệnh nhân",
                Result = $"Cập nhật thông tin cá nhân và {request.Conditions?.Count ?? 0} bệnh nền",
                UserId = user.Id
            });
            await _context.SaveChangesAsync();
        }
        catch { }

        // Fetch freshly updated user with relationships
        var updatedUser = await _context.Users
            .Include(u => u.PatientConditions)
                .ThenInclude(pc => pc.Condition)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        double bmi = 0;
        if (updatedUser?.Height > 0 && updatedUser?.Weight > 0)
        {
            double heightInMeters = updatedUser.Height.Value >= 3 ? updatedUser.Height.Value / 100 : updatedUser.Height.Value;
            bmi = updatedUser.Weight.Value / (heightInMeters * heightInMeters);
        }

        return Ok(new
        {
            id = updatedUser?.Id,
            fullName = updatedUser?.FullName ?? "",
            dateOfBirth = updatedUser?.BirthDate?.ToString("yyyy-MM-dd") ?? "",
            gender = updatedUser?.Gender ?? "other",
            weight = updatedUser?.Weight ?? 0.0,
            height = updatedUser?.Height ?? 0.0,
            notes = updatedUser?.Notes ?? "",
            conditions = updatedUser?.PatientConditions.Select(pc => pc.Condition?.Name ?? "").ToList() ?? new List<string>(),
            conditionIds = updatedUser?.PatientConditions.Select(pc => pc.ConditionId).ToList() ?? new List<int>(),
            medications = new List<string>(),
            bmi = bmi
        });
    }

    // 3. GET /api/patients
    [HttpGet]
    public async Task<IActionResult> ListPatients([FromQuery] string? q, [FromQuery] string role = "Patient")
    {
        if (role != "Patient" && role != "Doctor")
        {
            return BadRequest(new { message = "Vai trò không hợp lệ." });
        }

        var users = await _userManager.GetUsersInRoleAsync(role);
        var query = users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim().ToLower();
            query = query.Where(u => (u.FullName != null && u.FullName.ToLower().Contains(q)) 
                                  || (u.UserName != null && u.UserName.ToLower().Contains(q))
                                  || (u.Email != null && u.Email.ToLower().Contains(q)));
        }

        var results = new List<object>();
        foreach (var user in query)
        {
            var conditions = await _context.PatientConditions
                .Where(pc => pc.UserId == user.Id)
                .Include(pc => pc.Condition)
                .Select(pc => pc.Condition.Name)
                .ToListAsync();

            double bmi = 0;
            if (user.Height > 0 && user.Weight > 0)
            {
                double heightInMeters = user.Height.Value >= 3 ? user.Height.Value / 100 : user.Height.Value;
                bmi = user.Weight.Value / (heightInMeters * heightInMeters);
            }

            results.Add(new
            {
                id = user.Id,
                userName = user.UserName ?? "",
                email = user.Email ?? "",
                fullName = user.FullName ?? user.UserName,
                dateOfBirth = user.BirthDate?.ToString("yyyy-MM-dd") ?? "",
                gender = user.Gender ?? "other",
                weight = user.Weight ?? 0.0,
                height = user.Height ?? 0.0,
                notes = user.Notes ?? "",
                status = user.Status ?? "Active",
                createdAt = user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                conditions = conditions,
                bmi = bmi
            });
        }

        return Ok(results);
    }

    // 4. GET /api/patients/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatientById(string id)
    {
        var user = await _context.Users
            .Include(u => u.PatientConditions)
                .ThenInclude(pc => pc.Condition)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy bệnh nhân." });
        }

        double bmi = 0;
        if (user.Height > 0 && user.Weight > 0)
        {
            double heightInMeters = user.Height.Value >= 3 ? user.Height.Value / 100 : user.Height.Value;
            bmi = user.Weight.Value / (heightInMeters * heightInMeters);
        }

        return Ok(new
        {
            id = user.Id,
            fullName = user.FullName ?? user.UserName,
            dateOfBirth = user.BirthDate?.ToString("yyyy-MM-dd") ?? "",
            gender = user.Gender ?? "other",
            weight = user.Weight ?? 0.0,
            height = user.Height ?? 0.0,
            notes = user.Notes ?? "",
            conditions = user.PatientConditions.Select(pc => pc.Condition?.Name ?? "").ToList(),
            bmi = bmi
        });
    }

    // 5. PUT /api/patients/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePatientProfile(string id, [FromBody] UpdateProfileRequest request)
    {
        var user = await _context.Users
            .Include(u => u.PatientConditions)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy bệnh nhân." });
        }

        user.FullName = request.FullName;
        user.Notes = request.Notes;
        user.Gender = request.Gender;
        user.Weight = request.Weight;
        user.Height = request.Height;

        if (DateTime.TryParse(request.DateOfBirth, out DateTime dob))
        {
            user.BirthDate = dob;
        }

        _context.PatientConditions.RemoveRange(user.PatientConditions);
        await _context.SaveChangesAsync();

        if (request.Conditions != null && request.Conditions.Count > 0)
        {
            foreach (var condName in request.Conditions)
            {
                var cond = await _context.Conditions.FirstOrDefaultAsync(c => c.Name.ToLower() == condName.ToLower());
                if (cond != null)
                {
                    _context.PatientConditions.Add(new PatientCondition
                    {
                        UserId = user.Id,
                        ConditionId = cond.ConditionId
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        return Ok(new { message = "Cập nhật hồ sơ bệnh nhân thành công." });
    }

    // 6. POST /api/patients
    [HttpPost]
    public async Task<IActionResult> CreatePatientProfile([FromBody] CreatePatientRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Thông tin đăng ký không hợp lệ." });
        }

        string username = request.Username;
        if (string.IsNullOrWhiteSpace(username))
        {
            username = "patient_" + new Random().Next(100000, 999999);
        }

        string password = string.IsNullOrWhiteSpace(request.Password) ? "Patient123!" : request.Password;
        string email = string.IsNullOrWhiteSpace(request.Email) ? $"{username}@vietnhathospital.com" : request.Email;

        var existingUser = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Tên đăng nhập hoặc Email này đã tồn tại." });
        }

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            FullName = string.IsNullOrWhiteSpace(request.FullName) ? "Bệnh nhân mới" : request.FullName,
            Gender = request.Gender,
            Weight = request.Weight,
            Height = request.Height,
            Notes = request.Notes,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        if (DateTime.TryParse(request.DateOfBirth, out DateTime dob))
        {
            user.BirthDate = dob;
        }

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest(new { message = $"Tạo bệnh nhân thất bại: {errors}" });
        }

        await _userManager.AddToRoleAsync(user, "Patient");

        if (request.Conditions != null && request.Conditions.Count > 0)
        {
            foreach (var condName in request.Conditions)
            {
                var cond = await _context.Conditions.FirstOrDefaultAsync(c => c.Name.ToLower() == condName.ToLower());
                if (cond != null)
                {
                    _context.PatientConditions.Add(new PatientCondition
                    {
                        UserId = user.Id,
                        ConditionId = cond.ConditionId
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        return Ok(new { id = user.Id, fullName = user.FullName });
    }
}

public class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string Gender { get; set; } = "other";
    public double Weight { get; set; }
    public double Height { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<string> Conditions { get; set; } = new();
}

public class CreatePatientRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string Gender { get; set; } = "other";
    public double Weight { get; set; }
    public double Height { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<string> Conditions { get; set; } = new();
}
