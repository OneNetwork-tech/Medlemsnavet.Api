using Medlemsnavet.Data;
using Medlemsnavet.Dtos;
using Medlemsnavet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require login
public class MembersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MembersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/members (For regular members - shows only active members)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetMembers()
    {
        var members = await _context.Members
            .Where(m => m.DepartureDate == null)
            .Select(m => new MemberDto
            {
                Id = m.Id,
                Name = m.Name,
                Email = m.Email,
                PersonalIdentityNumber = m.PersonalIdentityNumber,
                PhoneNumber = m.PhoneNumber,
                PostalAddress = m.PostalAddress,
                EntryDate = m.EntryDate
            })
            .ToListAsync();

        return Ok(members);
    }

    // --- NEW ADMIN ENDPOINT ---
    // GET: api/members/admin/all (For Admins - shows all members)
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetAllMembersForAdmin()
    {
        var members = await _context.Members
            .Select(m => new MemberDto // We can create a more detailed AdminMemberDto later
            {
                Id = m.Id,
                Name = m.Name,
                Email = m.Email,
                PersonalIdentityNumber = m.PersonalIdentityNumber,
                PhoneNumber = m.PhoneNumber,
                PostalAddress = m.PostalAddress,
                EntryDate = m.EntryDate,
                IsActive = m.DepartureDate == null // Add a status field
            })
            .ToListAsync();

        return Ok(members);
    }

    // GET: api/members/admin/details/{id} (For Admins to get any member)
    [HttpGet("admin/details/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MemberDto>> GetMemberForAdmin(Guid id)
    {
        var member = await _context.Members
            .Where(m => m.Id == id)
            .Select(m => new MemberDto
            {
                Id = m.Id,
                Name = m.Name,
                Email = m.Email,
                PersonalIdentityNumber = m.PersonalIdentityNumber,
                PhoneNumber = m.PhoneNumber,
                PostalAddress = m.PostalAddress,
                EntryDate = m.EntryDate,
                IsActive = m.DepartureDate == null
            })
            .FirstOrDefaultAsync();

        if (member == null)
        {
            return NotFound();
        }

        return Ok(member);
    }


    // GET: api/members/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MemberDto>> GetMember(Guid id)
    {
        var member = await _context.Members
            .Where(m => m.DepartureDate == null && m.Id == id)
            .Select(m => new MemberDto
            {
                Id = m.Id,
                Name = m.Name,
                Email = m.Email,
                PersonalIdentityNumber = m.PersonalIdentityNumber,
                PhoneNumber = m.PhoneNumber,
                PostalAddress = m.PostalAddress,
                EntryDate = m.EntryDate
            })
            .FirstOrDefaultAsync();

        if (member == null)
        {
            return NotFound();
        }

        return Ok(member);
    }

    // POST: api/members
    [HttpPost]
    [Authorize(Roles = "Admin")] // Now protected
    public async Task<ActionResult<MemberDto>> CreateMember([FromBody] CreateMemberDto createMemberDto)
    {
        if (await _context.Members.AnyAsync(m => m.PersonalIdentityNumber == createMemberDto.PersonalIdentityNumber))
        {
            return BadRequest("A member with this personal identity number already exists.");
        }

        var member = new Member
        {
            Name = createMemberDto.Name,
            Email = createMemberDto.Email,
            PersonalIdentityNumber = createMemberDto.PersonalIdentityNumber,
            PhoneNumber = createMemberDto.PhoneNumber,
            PostalAddress = createMemberDto.PostalAddress,
            EntryDate = DateTime.UtcNow
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var memberDto = new MemberDto { /* ... mapping ... */ };
        return CreatedAtAction(nameof(GetMember), new { id = member.Id }, memberDto);
    }

    // PUT: api/members/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")] // Now protected
    public async Task<IActionResult> UpdateMember(Guid id, [FromBody] UpdateMemberDto updateMemberDto)
    {
        var member = await _context.Members.FirstOrDefaultAsync(m => m.Id == id);

        if (member == null)
        {
            return NotFound();
        }

        member.Name = updateMemberDto.Name;
        member.Email = updateMemberDto.Email;
        member.PhoneNumber = updateMemberDto.PhoneNumber;
        member.PostalAddress = updateMemberDto.PostalAddress;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/members/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Now protected
    public async Task<IActionResult> DeleteMember(Guid id)
    {
        var member = await _context.Members.FindAsync(id);

        if (member == null)
        {
            return NotFound();
        }

        // Soft delete: set the departure date
        member.DepartureDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
