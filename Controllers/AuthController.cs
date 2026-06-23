// Task 4 & 5: Hệ thống xác thực và Trang quản trị Admin - Thực hiện bởi Thành viên C
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VietNhatHospital.Models;

namespace VietNhatHospital.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    // 1. POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Yêu cầu đăng ký không hợp lệ." });
        }

        // Check if email already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Email này đã được đăng ký trong hệ thống." });
        }

        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email,
            FullName = request.FullName,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest(new { message = $"Đăng ký thất bại: {errors}" });
        }

        // Assign role
        var role = request.Role == "Doctor" ? "Doctor" : "Patient";
        if (await _roleManager.RoleExistsAsync(role))
        {
            await _userManager.AddToRoleAsync(user, role);
        }

        return Ok(new { message = "Đăng ký tài khoản thành công!" });
    }

    // 2. POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Thông tin đăng nhập không hợp lệ." });
        }

        // Find user by email or username
        var user = await _userManager.FindByEmailAsync(request.Username) 
                   ?? await _userManager.FindByNameAsync(request.Username);

        if (user == null)
        {
            return BadRequest(new { message = "Tài khoản hoặc mật khẩu không chính xác." });
        }

        var result = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!result)
        {
            return BadRequest(new { message = "Tài khoản hoặc mật khẩu không chính xác." });
        }

        // Get user role
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Patient";

        // Generate a mock token for frontend client authorization
        var mockToken = "user-id:" + user.Id;

        return Ok(new
        {
            token = mockToken,
            role = role,
            fullName = user.FullName ?? user.UserName
        });
    }

    // 3. POST /api/auth/logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { message = "Đăng xuất thành công." });
    }
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Patient"; // "Patient" or "Doctor"
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
