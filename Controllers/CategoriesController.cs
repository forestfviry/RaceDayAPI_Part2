using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db)
    {
        _db = db;
    }

    // GET /api/events/{id}/categories - public
    [HttpGet("events/{id:int}/categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEventCategories(int id)
    {
        var exists = await _db.Events.AnyAsync(e => e.EventID == id);
        if (!exists)
            return NotFound(new { message = "Event does not exist." });

        var categories = await _db.Categories
            .Where(c => c.EventID == id)
            .Select(c => new CategoryResponse
            {
                CategoryID = c.CategoryID,
                EventID = c.EventID,
                CategoryName = c.CategoryName,
                DistanceKm = c.DistanceKm,
                MaxParticipants = c.MaxParticipants,
                EntryFee = c.EntryFee
            })
            .ToListAsync();

        return Ok(categories);
    }

    // POST /api/events/{id}/categories - owning Organiser only
    [HttpPost("events/{id:int}/categories")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> AddCategory(int id, CategoryCreateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var evt = await _db.Events
            .Include(e => e.Organiser)
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (evt == null)
            return NotFound(new { message = "Event does not exist." });

        var organiserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        if (evt.OrganiserID != organiserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You are not the owning organiser of this event." });

        var category = new Category
        {
            EventID = id,
            CategoryName = request.CategoryName,
            DistanceKm = request.DistanceKm,
            MaxParticipants = request.MaxParticipants,
            EntryFee = request.EntryFee
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return Ok(new CategoryResponse
        {
            CategoryID = category.CategoryID,
            EventID = category.EventID,
            CategoryName = category.CategoryName,
            DistanceKm = category.DistanceKm,
            MaxParticipants = category.MaxParticipants,
            EntryFee = category.EntryFee
        });
    }

    // PUT /api/categories/{id} - owning Organiser only
    [HttpPut("categories/{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateCategory(int id, CategoryUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var category = await _db.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryID == id);

        if (category == null)
            return NotFound(new { message = "Category does not exist." });

        var organiserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        if (category.Event.OrganiserID != organiserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You are not the owning organiser of this event." });

        category.CategoryName = request.CategoryName;
        category.DistanceKm = request.DistanceKm;
        category.MaxParticipants = request.MaxParticipants;
        category.EntryFee = request.EntryFee;

        await _db.SaveChangesAsync();

        return Ok(new CategoryResponse
        {
            CategoryID = category.CategoryID,
            EventID = category.EventID,
            CategoryName = category.CategoryName,
            DistanceKm = category.DistanceKm,
            MaxParticipants = category.MaxParticipants,
            EntryFee = category.EntryFee
        });
    }

    // DELETE /api/categories/{id} - owning Organiser only
    [HttpDelete("categories/{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _db.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryID == id);

        if (category == null)
            return NotFound(new { message = "Category does not exist." });

        var organiserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        if (category.Event.OrganiserID != organiserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You are not the owning organiser of this event." });

        _db.Categories.Remove(category); // enrolments cascade
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
