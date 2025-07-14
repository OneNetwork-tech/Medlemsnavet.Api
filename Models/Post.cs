// Models/Post.cs
using Medlemsnavet.Models;

public class Post
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Guid MemberId { get; set; } // Must be Guid
    public Member Author { get; set; } = null!;
    public string? ImageUrl { get; set; }
}