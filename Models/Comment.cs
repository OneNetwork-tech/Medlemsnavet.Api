using System.ComponentModel.DataAnnotations;

namespace Medlemsnavet.Models;

public class Comment
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(1000)]
    public string Content { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    // Foreign key to the Post
    public int PostId { get; set; }
    public Post Post { get; set; }

    // Foreign key to the Member who commented
    public Guid MemberId { get; set; }
    public Member Author { get; set; }
}