using System.ComponentModel.DataAnnotations;

namespace Medlemsnavet.Dtos;

public class CreateInvoiceDto
{
    [Required]
    public Guid MemberId { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; }

    [Required]
    [Range(0.01, 100000.00)]
    public decimal Amount { get; set; }

    [Required]
    public DateTime DueDate { get; set; }
}