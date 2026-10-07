using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // any logged-in user
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public UsersController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /api/users/me
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserID == CurrentUserId);

        if (user == null)
            return Unauthorized(new { message = "No/invalid token." });

        return Ok(new UserResponse
        {
            UserID = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.RoleName,
            CreatedAt = user.CreatedAt
        });
    }

    // PUT /api/users/me
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserID == CurrentUserId);

        if (user == null)
            return Unauthorized(new { message = "No/invalid token." });

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;

        await _db.SaveChangesAsync();

        return Ok(new UserResponse
        {
            UserID = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.RoleName,
            CreatedAt = user.CreatedAt
        });
    }
}
