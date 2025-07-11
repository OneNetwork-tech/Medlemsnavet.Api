using System.ComponentModel.DataAnnotations;

namespace Medlemsnavet.Models;

public class Event
{
    [Key]
    public int Id { get; set; }

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

    public DateTime CreatedAt { get; set; }

    // Foreign key to the Member who created the event
    public Guid CreatorId { get; set; }
    public Member Creator { get; set; }

    // Many-to-many relationship for attendees
    public ICollection<Member> Attendees { get; set; } = new List<Member>();
}