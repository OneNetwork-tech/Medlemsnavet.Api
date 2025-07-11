namespace Medlemsnavet.Dtos;

public class PostDto
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; }
}