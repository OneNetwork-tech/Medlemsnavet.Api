using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medlemsnavet.Models;

public class Invoice
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } // e.g., "Membership Fee 2025"

    [Required]
    [Column(TypeName = "decimal(18, 2)")] // Best practice for storing money
    public decimal Amount { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public DateTime? PaidDate { get; set; }

    [Required]
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

    // Foreign key to the Member this invoice belongs to
    public Guid MemberId { get; set; }
    public Member Member { get; set; }
}