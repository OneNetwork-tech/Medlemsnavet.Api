using Microsoft.AspNetCore.Identity;

namespace Medlemsnavet.Data;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = { "Admin", "Member" };
        foreach (var roleName in roleNames)
        {
            // Check if the role already exists
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                // Create the role
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}