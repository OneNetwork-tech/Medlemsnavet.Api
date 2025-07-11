using System.ComponentModel.DataAnnotations;

namespace Medlemsnavet.Dtos;

public class CreateCommentDto
{
    [Required]
    [StringLength(1000)]
    public string Content { get; set; }
}