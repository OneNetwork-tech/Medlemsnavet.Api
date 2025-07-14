// Dtos/PostDto.cs
public class PostDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid AuthorId { get; set; } // Changed from int to Guid
    public string AuthorName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}