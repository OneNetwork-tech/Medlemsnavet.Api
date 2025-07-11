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
public class InvoicesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public InvoicesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/invoices/my-invoices (For members to see their own invoices)
    [HttpGet("my-invoices")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetMyInvoices()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Name);
        var appUser = await _userManager.FindByEmailAsync(userEmail);

        if (appUser?.MemberId == null)
        {
            return Forbid("You are not linked to a member profile.");
        }

        var invoices = await _context.Invoices
            .Where(i => i.MemberId == appUser.MemberId.Value)
            .OrderByDescending(i => i.DueDate)
            .Select(i => new InvoiceDto
            {
                Id = i.Id,
                Title = i.Title,
                Amount = i.Amount,
                DueDate = i.DueDate,
                PaidDate = i.PaidDate,
                Status = i.Status.ToString()
            })
            .ToListAsync();

        return Ok(invoices);
    }

    // POST: api/invoices (Admin action to create an invoice)
    [HttpPost]
    [Authorize(Roles = "Admin")] // Lock down to admins
    public async Task<ActionResult> CreateInvoice([FromBody] CreateInvoiceDto createInvoiceDto)
    {
        var memberExists = await _context.Members.AnyAsync(m => m.Id == createInvoiceDto.MemberId);
        if (!memberExists)
        {
            return BadRequest("Member not found.");
        }

        var invoice = new Invoice
        {
            MemberId = createInvoiceDto.MemberId,
            Title = createInvoiceDto.Title,
            Amount = createInvoiceDto.Amount,
            DueDate = createInvoiceDto.DueDate,
            Status = InvoiceStatus.Pending
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Here you would later trigger an email to the member

        return Ok("Invoice created successfully.");
    }

    // POST: api/invoices/5/mark-paid (Admin action)
    [HttpPost("{id}/mark-paid")]
    // [Authorize(Roles = "Admin")] // Use this to lock down to admins
    public async Task<IActionResult> MarkInvoiceAsPaid(int id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null)
        {
            return NotFound();
        }

        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Here you would later trigger a "payment received" email

        return Ok("Invoice marked as paid.");
    }
}