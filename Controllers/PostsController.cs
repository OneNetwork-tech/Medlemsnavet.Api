using Medlemsnavet.Data;
using Medlemsnavet.Dtos;
using Medlemsnavet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All actions here require a logged-in user
public class PostsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PostsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/posts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPosts()
    {
        var posts = await _context.Posts
            .Include(p => p.Author) // Include the author's data
            .Where(p => p.Author.DepartureDate == null) // Only show posts from active members
            .OrderByDescending(p => p.CreatedAt) // Show newest posts first
            .Select(p => new PostDto
            {
                Id = p.Id,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                AuthorId = p.Author.Id,
                AuthorName = p.Author.Name
            })
            .ToListAsync();

        return Ok(posts);
    }

    // POST: api/posts
    [HttpPost]
    public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostDto createPostDto)
    {
        // Find the user ID from the JWT token
        var userEmail = User.FindFirstValue(ClaimTypes.Name);
        if (userEmail == null)
        {
            return Unauthorized(); // Should not happen if [Authorize] is working
        }

        var applicationUser = await _userManager.FindByEmailAsync(userEmail);
        if (applicationUser == null || applicationUser.MemberId == null)
        {
            // This user is not linked to a member profile and cannot post.
            return Forbid("You must be a registered member to create a post.");
        }

        var post = new Post
        {
            Content = createPostDto.Content,
            CreatedAt = DateTime.UtcNow,
            MemberId = applicationUser.MemberId.Value // Use the MemberId from the user's login
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Load the author's name for the response
        var author = await _context.Members.FindAsync(post.MemberId);

        var postDto = new PostDto
        {
            Id = post.Id,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            AuthorId = post.MemberId,
            AuthorName = author?.Name ?? "Unknown"
        };

        return CreatedAtAction(nameof(GetPosts), postDto);
    }
}