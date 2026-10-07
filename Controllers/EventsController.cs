using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EventsController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /api/events - lists all upcoming events (public)
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetUpcomingEvents()
    {
        var today = DateTime.Today;
        var events = await _db.Events
            .Include(e => e.Organiser)
            .Where(e => e.EventDate >= today)
            .OrderBy(e => e.EventDate)
            .Select(e => new EventResponse
            {
                EventID = e.EventID,
                EventName = e.EventName,
                EventDate = e.EventDate,
                Location = e.Location,
                Description = e.Description,
                OrganiserID = e.OrganiserID,
                OrganiserName = e.Organiser.FullName
            })
            .ToListAsync();

        return Ok(events);
    }

    // GET /api/events/{id} - public
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEvent(int id)
    {
        var evt = await _db.Events
            .Include(e => e.Organiser)
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (evt == null)
            return NotFound(new { message = "Event does not exist." });

        return Ok(new EventResponse
        {
            EventID = evt.EventID,
            EventName = evt.EventName,
            EventDate = evt.EventDate,
            Location = evt.Location,
            Description = evt.Description,
            OrganiserID = evt.OrganiserID,
            OrganiserName = evt.Organiser.FullName
        });
    }

    // POST /api/events - Organiser only
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateEvent(EventCreateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var evt = new Event
        {
            OrganiserID = CurrentUserId,
            EventName = request.EventName,
            EventDate = request.EventDate.Date,
            Location = request.Location,
            Description = request.Description,
            CreatedAt = DateTime.Now
        };

        _db.Events.Add(evt);
        await _db.SaveChangesAsync();

        var organiser = await _db.Users.FindAsync(CurrentUserId);

        return StatusCode(StatusCodes.Status201Created, new EventResponse
        {
            EventID = evt.EventID,
            EventName = evt.EventName,
            EventDate = evt.EventDate,
            Location = evt.Location,
            Description = evt.Description,
            OrganiserID = evt.OrganiserID,
            OrganiserName = organiser?.FullName ?? string.Empty
        });
    }

    // PUT /api/events/{id} - owning Organiser only
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateEvent(int id, EventUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var evt = await _db.Events
            .Include(e => e.Organiser)
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (evt == null)
            return NotFound(new { message = "Event does not exist." });

        if (evt.OrganiserID != CurrentUserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You are not the owning organiser of this event." });

        evt.EventName = request.EventName;
        evt.EventDate = request.EventDate.Date;
        evt.Location = request.Location;
        evt.Description = request.Description;

        await _db.SaveChangesAsync();

        return Ok(new EventResponse
        {
            EventID = evt.EventID,
            EventName = evt.EventName,
            EventDate = evt.EventDate,
            Location = evt.Location,
            Description = evt.Description,
            OrganiserID = evt.OrganiserID,
            OrganiserName = evt.Organiser.FullName
        });
    }

    // DELETE /api/events/{id} - owning Organiser only
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var evt = await _db.Events.FindAsync(id);

        if (evt == null)
            return NotFound(new { message = "Event does not exist." });

        if (evt.OrganiserID != CurrentUserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You are not the owning organiser of this event." });

        _db.Events.Remove(evt); // categories + enrolments cascade
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
