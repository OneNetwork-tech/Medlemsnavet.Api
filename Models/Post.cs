using System.ComponentModel.DataAnnotations;
namespace Medlemsnavet.Models;

//namespace Medlemsnavet.Api.Models 

public class Post
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Content { get; set; }
    public string? ImageUrl { get; set; } // URL to an image in blob storage
    public string? VideoUrl { get; set; } // URL to a video

    [Required]
    public DateTime CreatedAt { get; set; }

    // Foreign key to the member who created it
    public Guid MemberId { get; set; }
    public Member Author { get; set; }
}