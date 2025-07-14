using System.ComponentModel.DataAnnotations;

namespace Medlemsnavet.Dtos;

public class CreatePostDto
{
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}