using Medlemsnavet.Data;
using Medlemsnavet.Dtos;
using Medlemsnavet.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger; // Inject the logger

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ApplicationDbContext context,
        ILogger<AuthController> logger) // Add ILogger here
    {
        _userManager = userManager;
        _configuration = configuration;
        _context = context;
        _logger = logger; // And here
    }

    // NEW: Combined registration endpoint
    [HttpPost("register-member")]
    public async Task<IActionResult> RegisterMember([FromBody] RegisterMemberDto registerDto)
    {
        _logger.LogInformation("Registration attempt for email {Email}", registerDto.Email);

        // Check for existing user by email or personnummer
        var userExists = await _userManager.FindByEmailAsync(registerDto.Email);
        if (userExists != null)
        {
            _logger.LogWarning("Registration failed: Email {Email} already exists.", registerDto.Email);
            return BadRequest("An account with this email already exists.");
        }
        var memberExists = await _context.Members.AnyAsync(m => m.PersonalIdentityNumber == registerDto.PersonalIdentityNumber);
        if (memberExists)
        {
            _logger.LogWarning("Registration failed: Personnummer {Pnr} already exists.", registerDto.PersonalIdentityNumber);
            return BadRequest("A member with this personal identity number already exists.");
        }

        // --- Start a database transaction to ensure both creations succeed or fail together ---
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Create the login user
            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                _logger.LogError("User creation failed for {Email}: {Errors}", registerDto.Email, result.Errors);
                return BadRequest(result.Errors);
            }

            // 2. Create the member profile
            var member = new Member
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                PersonalIdentityNumber = registerDto.PersonalIdentityNumber,
                PhoneNumber = registerDto.PhoneNumber,
                PostalAddress = registerDto.PostalAddress,
                EntryDate = DateTime.UtcNow,
                ApplicationUserId = user.Id // Link to the new user
            };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            // 3. Assign the user a role
            if (_userManager.Users.Count() == 1)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                _logger.LogInformation("User {Email} created and assigned Admin role.", user.Email);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Member");
                _logger.LogInformation("User {Email} created and assigned Member role.", user.Email);
            }

            // If everything is successful, commit the transaction
            await transaction.CommitAsync();

            return Ok(new { Message = "User and member profile created successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during registration for {Email}", registerDto.Email);
            // If any error occurs, roll back the entire operation
            await transaction.RollbackAsync();
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred during registration.");
        }
    }


    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user != null && await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            _logger.LogInformation("User {Email} logged in successfully.", loginDto.Email);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                roles = userRoles
            });
        }

        _logger.LogWarning("Failed login attempt for user {Email}", loginDto.Email);
        return Unauthorized("Invalid credentials.");
    }
}
