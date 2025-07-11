// File: Data/ApplicationDbContext.cs

using Medlemsnavet.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


// Inherit from IdentityDbContext to include all of ASP.NET Identity's tables (Users, Roles, etc.)
namespace Medlemsnavet.Models;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // A DbSet represents a table in the database.
    // We need one for Members and one for Posts.
    public DbSet<Member> Members { get; set; }
    public DbSet<Post> Posts { get; set; }

    // Add this line inside ApplicationDbContext.cs
    public DbSet<Comment> Comments { get; set; }

    // This method is used to configure relationships between tables.
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // This is crucial for Identity tables to be configured correctly.
        base.OnModelCreating(builder);

        // Explicitly configure the one-to-one relationship between
        // ApplicationUser (the login) and Member (the association data).
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Member)
            .WithOne(m => m.ApplicationUser)
            .HasForeignKey<Member>(m => m.ApplicationUserId);
    }
}