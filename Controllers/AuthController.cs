using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;
using RaceDayAPI.Services;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(RaceDayDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Role must be either Organiser or Participant
        var roleName = request.Role.Trim();
        if (!roleName.Equals("Organiser", StringComparison.OrdinalIgnoreCase) &&
            !roleName.Equals("Participant", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Role", "Role must be either 'Organiser' or 'Participant'.");
            return BadRequest(ModelState);
        }

        // Email must be unique
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return Conflict(new { message = "Email already registered." });

        var role = await _db.Roles.FirstAsync(r => r.RoleName == roleName);

        var user = new User
        {
            RoleID = role.RoleID,
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = null,
            CreatedAt = DateTime.Now
        };

        // Hash the password (never store plain text)
        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var response = new UserResponse
        {
            UserID = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = role.RoleName,
            CreatedAt = user.CreatedAt
        };

        return StatusCode(StatusCodes.Status201Created, response);
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return Unauthorized(new { message = "Invalid credentials." });

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid credentials." });

        var token = _tokenService.CreateToken(user, user.Role.RoleName);

        var response = new LoginResponse
        {
            Token = token,
            User = new UserResponse
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.RoleName,
                CreatedAt = user.CreatedAt
            }
        };

        return Ok(response);
    }
}
