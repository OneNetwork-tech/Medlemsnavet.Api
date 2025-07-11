using Medlemsnavet.Data;
using Medlemsnavet.Dtos;
using Medlemsnavet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/posts/{postId}/comments")] // Nested route under posts
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CommentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/posts/{postId}/comments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsForPost(int postId)
    {
        var comments = await _context.Comments
            .Where(c => c.PostId == postId)
            .Include(c => c.Author)
            .OrderBy(c => c.CreatedAt) // Show oldest comments first
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                AuthorId = c.Author.Id,
                AuthorName = c.Author.Name
            })
            .ToListAsync();

        return Ok(comments);
    }

    // POST: api/posts/{postId}/comments
    [HttpPost]
    public async Task<ActionResult<CommentDto>> CreateComment(int postId, [FromBody] CreateCommentDto createCommentDto)
    {
        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
        {
            return NotFound("Post not found.");
        }

        var userEmail = User.FindFirstValue(ClaimTypes.Name);
        var applicationUser = await _userManager.FindByEmailAsync(userEmail);
        if (applicationUser == null || applicationUser.MemberId == null)
        {
            return Forbid("You must be a registered member to comment.");
        }

        var comment = new Comment
        {
            Content = createCommentDto.Content,
            CreatedAt = DateTime.UtcNow,
            PostId = postId,
            MemberId = applicationUser.MemberId.Value
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Load the author for the response DTO
        await _context.Entry(comment).Reference(c => c.Author).LoadAsync();

        var commentDto = new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            AuthorId = comment.MemberId,
            AuthorName = comment.Author.Name
        };

        return CreatedAtAction(nameof(GetCommentsForPost), new { postId = postId }, commentDto);
    }
}