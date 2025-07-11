using Medlemsnavet.Models;

namespace Medlemsnavet.Dtos;

public class InvoiceDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string Status { get; set; }
}