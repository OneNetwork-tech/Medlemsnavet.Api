// File: Models/Member.cs (Updated)
using System.ComponentModel.DataAnnotations;
using Medlemsnavet.Validation; // We will create this namespace
namespace Medlemsnavet.Models;

public class Member
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [EmailAddress] // Use built-in email validation
    [StringLength(100)]
    public string Email { get; set; }

    // --- NEW FIELDS ---
    [Required]
    [Personnummer] // Our custom validation attribute
    [StringLength(13)]
    public string PersonalIdentityNumber { get; set; } // Stores YYYYMMDD-XXXX

    [Required]
    [Phone] // Use built-in phone validation
    [StringLength(20)]
    public string PhoneNumber { get; set; }

    [Required]
    [StringLength(200)]
    public string PostalAddress { get; set; } // This will be populated from the lookup service

    // --- EXISTING FIELDS ---
    [Required]
    public DateTime EntryDate { get; set; }
    public DateTime? DepartureDate { get; set; }

    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}