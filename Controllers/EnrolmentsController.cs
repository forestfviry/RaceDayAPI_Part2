using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static EnrolmentResponse Map(Enrolment e) => new()
    {
        EnrolmentID = e.EnrolmentID,
        UserID = e.UserID,
        ParticipantName = e.User.FullName,
        ParticipantEmail = e.User.Email,
        CategoryID = e.CategoryID,
        CategoryName = e.Category.CategoryName,
        EventName = e.Category.Event.EventName,
        EnrolmentDate = e.EnrolmentDate,
        Status = e.Status
    };

    // POST /api/categories/{id}/enrol - Participant only
    [HttpPost("categories/{id:int}/enrol")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Enrol(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null)
            return NotFound(new { message = "Category does not exist." });

        // Already enrolled in this category?
        var alreadyEnrolled = await _db.Enrolments
            .AnyAsync(e => e.UserID == CurrentUserId && e.CategoryID == id);
        if (alreadyEnrolled)
            return Conflict(new { message = "You are already enrolled in this category." });

        // Category full?
        var enrolledCount = await _db.Enrolments
            .CountAsync(e => e.CategoryID == id);
        if (enrolledCount >= category.MaxParticipants)
            return Conflict(new { message = "This category is full." });

        var enrolment = new Enrolment
        {
            UserID = CurrentUserId,
            CategoryID = id,
            EnrolmentDate = DateTime.Now,
            Status = "Registered"
        };

        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        var saved = await _db.Enrolments
            .Include(e => e.User)
            .Include(e => e.Category)
            .ThenInclude(c => c.Event)
            .FirstAsync(e => e.EnrolmentID == enrolment.EnrolmentID);

        return StatusCode(StatusCodes.Status201Created, Map(saved));
    }

    // GET /api/users/me/enrolments - Participant only
    [HttpGet("users/me/enrolments")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyEnrolments()
    {
        var enrolments = await _db.Enrolments
            .Include(e => e.User)
            .Include(e => e.Category)
            .ThenInclude(c => c.Event)
            .Where(e => e.UserID == CurrentUserId)
            .OrderByDescending(e => e.EnrolmentDate)
            .ToListAsync();

        return Ok(enrolments.Select(Map));
    }

    // GET /api/categories/{id}/enrolments - owning Organiser only
    [HttpGet("categories/{id:int}/enrolments")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetCategoryEnrolments(int id)
    {
        var category = await _db.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryID == id);

        if (category == null)
            return NotFound(new { message = "Category does not exist." });

        if (category.Event.OrganiserID != CurrentUserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You are not the owning organiser of this event." });

        var enrolments = await _db.Enrolments
            .Include(e => e.User)
            .Include(e => e.Category)
            .ThenInclude(c => c.Event)
            .Where(e => e.CategoryID == id)
            .OrderBy(e => e.EnrolmentDate)
            .ToListAsync();

        return Ok(enrolments.Select(Map));
    }

    // DELETE /api/enrolments/{id} - the enrolled Participant only
    [HttpDelete("enrolments/{id:int}")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        var enrolment = await _db.Enrolments.FindAsync(id);

        if (enrolment == null)
            return NotFound(new { message = "Enrolment does not exist." });

        if (enrolment.UserID != CurrentUserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only cancel your own enrolment." });

        _db.Enrolments.Remove(enrolment); // result cascades
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
