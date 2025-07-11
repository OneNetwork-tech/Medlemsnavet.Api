using System.ComponentModel.DataAnnotations;

namespace Medlemsnavet.Dtos;

public class CreateEventDto
{
    [Required]
    [StringLength(100)]
    public string Title { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    [StringLength(200)]
    public string Location { get; set; }
}