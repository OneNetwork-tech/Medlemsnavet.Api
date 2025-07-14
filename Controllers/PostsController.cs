using System.Net.NetworkInformation;
using Medlemsnavet.Data;
using Medlemsnavet.Dtos;
using Medlemsnavet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR.Client;
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
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPosts(int page = 1, int pageSize = 20)
    {
        var posts = await _context.Posts
            .Include(p => p.Author)
            .Where(p => p.Author.DepartureDate == null)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PostDto
            {
                Id = p.Id,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                AuthorId = p.MemberId,
                AuthorName = p.Author.Name,
                ImageUrl = p.ImageUrl
            })
            .ToListAsync();

        return Ok(posts);
    }

    // GET: api/posts/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<PostDto>> GetPostById(int id)
    {
        var post = await _context.Posts
            .Include(p => p.Author)
            .Where(p => p.Author.DepartureDate == null)
            .Select(p => new PostDto
            {
                Id = p.Id,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                AuthorId = p.MemberId,
                AuthorName = p.Author.Name,
                ImageUrl = p.ImageUrl
            })
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound(new { Message = "Post not found or author is no longer active." });
        }

        return Ok(post);
    }

    // POST: api/posts
    [HttpPost]
    public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostDto createPostDto)
    {
        if (string.IsNullOrWhiteSpace(createPostDto.Content))
        {
            return BadRequest(new { Message = "Content cannot be empty." });
        }

        var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        if (string.IsNullOrEmpty(userEmail))
        {
            return Unauthorized(new { Message = "User not authenticated." });
        }

        var applicationUser = await _userManager.FindByEmailAsync(userEmail);
        if (applicationUser == null || applicationUser.MemberId == null)
        {
            return Forbid(new { Message = "You must be a registered member to create a post." });
        }

        var post = new Post
        {
            Content = createPostDto.Content,
            CreatedAt = DateTime.UtcNow,
            MemberId = applicationUser.MemberId.Value,
            ImageUrl = createPostDto.ImageUrl?.Trim()
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var author = await _context.Members.FindAsync(post.MemberId);

        var postDto = new PostDto
        {
            Id = post.Id,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            AuthorId = post.MemberId,
            AuthorName = author?.Name ?? "Unknown",
            ImageUrl = post.ImageUrl
        };

        return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, postDto);
    }
}