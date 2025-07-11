using System.Text;
using Medlemsnavet.Data;
using Medlemsnavet.Hubs;
using Medlemsnavet.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// --- 1. Add services to the container ---

// Configure CORS Policy
var MyAllowSpecificOrigins = "AllowBlazorApp";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            // Replace with your Blazor app's actual URL
            policy.WithOrigins("https://localhost:7057")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Add the DbContext and configure it to use PostgreSQL
var connectionString = configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add ASP.NET Core Identity for user and role management
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add and configure JWT Bearer Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false; // Set to true in production
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = configuration["JWT:ValidAudience"],
            ValidIssuer = configuration["JWT:ValidIssuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]))
        };
    });

builder.Services.AddControllers();
builder.Services.AddSignalR();

// Add Swagger/OpenAPI for API documentation and testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// --- 2. Build the application ---
var app = builder.Build();


// --- 3. Seed Roles ---
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedRolesAsync(roleManager);
}


// --- 4. Configure the HTTP request pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

// Use the CORS policy
app.UseCors(MyAllowSpecificOrigins);

// The order of these is important
app.UseAuthentication(); // First, who are you?
app.UseAuthorization();  // Second, what are you allowed to do?

// Map the controllers and hubs
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");


// --- 5. Run the application ---
app.Run();
