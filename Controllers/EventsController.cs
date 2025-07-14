using Medlemsnavet.Data;
using Medlemsnavet.Dtos;
using Medlemsnavet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public EventsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/events (For regular members)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
    {
        var events = await _context.Events
            .Include(e => e.Creator)
            .Include(e => e.Attendees)
            .Where(e => e.StartTime >= DateTime.UtcNow)
            .OrderBy(e => e.StartTime)
            .Select(e => new EventDto { /* ... mapping ... */ })
            .ToListAsync();

        return Ok(events);
    }

    // --- NEW ADMIN ENDPOINT ---
    // GET: api/events/admin/all
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetAllEventsForAdmin()
    {
        var events = await _context.Events
            .Include(e => e.Creator)
            .Include(e => e.Attendees)
            .OrderByDescending(e => e.StartTime) // Show all, newest first
            .Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Location = e.Location,
                CreatorName = e.Creator.Name,
                AttendeeCount = e.Attendees.Count
            })
            .ToListAsync();

        return Ok(events);
    }

    // POST: api/events
    [HttpPost]
    public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventDto createEventDto)
    {
        // ... existing code ...
        var user = await GetCurrentMemberAsync();
        if (user == null) return Forbid();

        var newEvent = new Event
        {
            Title = createEventDto.Title,
            Description = createEventDto.Description,
            Location = createEventDto.Location,
            StartTime = createEventDto.StartTime,
            EndTime = createEventDto.EndTime,
            CreatedAt = DateTime.UtcNow,
            CreatorId = user.Id
        };

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEvents), new { id = newEvent.Id }, null);
    }

    // POST: api/events/{id}/rsvp
    [HttpPost("{id}/rsvp")]
    public async Task<IActionResult> RsvpToEvent(int id)
    {
        // ... existing code ...
        var user = await GetCurrentMemberAsync();
        if (user == null) return Forbid();

        var anEvent = await _context.Events.Include(e => e.Attendees).FirstOrDefaultAsync(e => e.Id == id);
        if (anEvent == null) return NotFound();

        if (!anEvent.Attendees.Any(a => a.Id == user.Id))
        {
            anEvent.Attendees.Add(user);
            await _context.SaveChangesAsync();
        }

        return Ok("RSVP successful.");
    }

    // --- NEW ADMIN ENDPOINT ---
    // DELETE: api/events/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var anEvent = await _context.Events.FindAsync(id);
        if (anEvent == null)
        {
            return NotFound();
        }

        _context.Events.Remove(anEvent);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Helper method
    private async Task<Member?> GetCurrentMemberAsync()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrEmpty(userEmail)) return null;

        var appUser = await _userManager.FindByEmailAsync(userEmail);
        if (appUser?.MemberId == null) return null;

        return await _context.Members.FindAsync(appUser.MemberId.Value);
    }
}
