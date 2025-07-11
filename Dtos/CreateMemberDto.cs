using Medlemsnavet.Validation;
using System.ComponentModel.DataAnnotations;

namespace Medlemsnavet.Dtos;

public class CreateMemberDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [Personnummer]
    public string PersonalIdentityNumber { get; set; }

    [Required]
    [Phone]
    public string PhoneNumber { get; set; }

    [Required]
    public string PostalAddress { get; set; }
}