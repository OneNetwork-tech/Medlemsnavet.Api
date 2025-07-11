using Medlemsnavet.Data;
using Medlemsnavet.Dtos;
using Medlemsnavet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medlemsnavet.Data;

[ApiController]
[Route("api/[controller]")]
[Authorize] // This entire controller is now protected. A valid JWT is required.
public class MembersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MembersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/members
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetMembers()
    {
        var members = await _context.Members
            .Where(m => m.DepartureDate == null) // Only get active members
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

    // GET: api/members/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MemberDto>> GetMember(Guid id)
    {
        var member = await _context.Members
            .Where(m => m.DepartureDate == null) // Only get active members
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
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member == null)
        {
            return NotFound();
        }

        return Ok(member);
    }

    // POST: api/members
    [HttpPost]
    // [Authorize(Roles = "Admin")] // You can uncomment this later to restrict access to admins
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

        var memberDto = new MemberDto
        {
            Id = member.Id,
            Name = member.Name,
            Email = member.Email,
            PersonalIdentityNumber = member.PersonalIdentityNumber,
            PhoneNumber = member.PhoneNumber,
            PostalAddress = member.PostalAddress,
            EntryDate = member.EntryDate
        };

        return CreatedAtAction(nameof(GetMember), new { id = member.Id }, memberDto);
    }

    // PUT: api/members/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(Guid id, [FromBody] UpdateMemberDto updateMemberDto)
    {
        var member = await _context.Members.FirstOrDefaultAsync(m => m.Id == id && m.DepartureDate == null);

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
    public async Task<IActionResult> DeleteMember(Guid id)
    {
        var member = await _context.Members.FindAsync(id);

        if (member == null)
        {
            return NotFound();
        }

        // Soft delete: set the departure date instead of removing the record
        member.DepartureDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}