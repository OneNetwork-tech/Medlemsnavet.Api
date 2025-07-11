using Medlemsnavet.Validation;
using System.ComponentModel.DataAnnotations;

namespace Medlemsnavet.Dtos;

public class RegisterMemberDto
{
    [Required]
    public string Name { get; set; }

    [Required]
    [Personnummer]
    public string PersonalIdentityNumber { get; set; }

    [Required]
    [Phone]
    public string PhoneNumber { get; set; }

    [Required]
    public string PostalAddress { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}