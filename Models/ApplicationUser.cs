// File: Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
namespace Medlemsnavet.Models;

// Extends the built-in IdentityUser with a link to our Member data
public class ApplicationUser : IdentityUser
{
    // Foreign key to the Member entity
    public Guid? MemberId { get; set; }
    public Member Member { get; set; }
}