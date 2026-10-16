using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    private static ResultResponse Map(Result r) => new()
    {
        ResultID = r.ResultID,
        EnrolmentID = r.EnrolmentID,
        FinishTime = r.FinishTime?.ToString(@"hh\:mm\:ss"),
        Position = r.Position,
        Status = r.Status,
        ParticipantName = r.Enrolment.User.FullName
    };

    private bool TryParseFinishTime(string value, out TimeSpan finishTime)
    {
        return TimeSpan.TryParse(value, out finishTime) && finishTime > TimeSpan.Zero;
    }

    // POST /api/enrolments/{id}/result - owning Organiser only
    [HttpPost("enrolments/{id:int}/result")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CaptureResult(int id, ResultCreateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!TryParseFinishTime(request.FinishTime, out var finishTime))
        {
            ModelState.AddModelError("FinishTime", "FinishTime must be in hh:mm:ss format, e.g. \"00:24:35\".");
            return BadRequest(ModelState);
        }

        var enrolment = await _db.Enrolments
            .Include(e => e.Category)
            .ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentID == id);

        if (enrolment == null)
            return NotFound(new { message = "Enrolment does not exist." });

        if (enrolment.Category.Event.OrganiserID != CurrentUserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You are not the owning organiser of this event." });

        // One result per enrolment (EnrolmentID is UNIQUE in the schema)
        var existing = await _db.Results.AnyAsync(r => r.EnrolmentID == id);
        if (existing)
            return Conflict(new { message = "A result already exists for this enrolment." });

        var result = new Result
        {
            EnrolmentID = id,
            FinishTime = finishTime,
            Position = request.Position,
            Status = request.Status
        };

        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        var saved = await _db.Results
            .Include(r => r.Enrolment)
            .ThenInclude(e => e.User)
            .FirstAsync(r => r.ResultID == result.ResultID);

        return StatusCode(StatusCodes.Status201Created, Map(saved));
    }

    // PUT /api/results/{id} - owning Organiser only
    [HttpPut("results/{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateResult(int id, ResultUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!TryParseFinishTime(request.FinishTime, out var finishTime))
        {
            ModelState.AddModelError("FinishTime", "FinishTime must be in hh:mm:ss format, e.g. \"00:24:35\".");
            return BadRequest(ModelState);
        }

        var result = await _db.Results
            .Include(r => r.Enrolment)
            .ThenInclude(e => e.Category)
            .ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(r => r.ResultID == id);

        if (result == null)
            return NotFound(new { message = "Result does not exist." });

        if (result.Enrolment.Category.Event.OrganiserID != CurrentUserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You are not the owning organiser of this event." });

        result.FinishTime = finishTime;
        result.Position = request.Position;
        result.Status = request.Status;

        await _db.SaveChangesAsync();

        var saved = await _db.Results
            .Include(r => r.Enrolment)
            .ThenInclude(e => e.User)
            .FirstAsync(r => r.ResultID == result.ResultID);

        return Ok(Map(saved));
    }

    // GET /api/categories/{id}/result - public leaderboard, ordered by position
    [HttpGet("categories/{id:int}/result")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategoryResults(int id)
    {
        var categoryExists = await _db.Categories.AnyAsync(c => c.CategoryID == id);
        if (!categoryExists)
            return NotFound(new { message = "Category does not exist." });

        var results = await _db.Results
            .Include(r => r.Enrolment)
            .ThenInclude(e => e.User)
            .Where(r => r.Enrolment.CategoryID == id)
            .OrderBy(r => r.Position)
            .ToListAsync();

        return Ok(results.Select(Map));

        // GET /api/users/me/results - Participant only
        [HttpGet("users/me/results")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var results = await _db.Results
                .Include(r => r.Enrolment).ThenInclude(e => e.User)
                .Where(r => r.Enrolment.UserID == CurrentUserId)
                .ToListAsync();
            return Ok(results.Select(Map));
        }
    }
}
