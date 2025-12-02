using Microsoft.AspNetCore.Identity;
using MobileAPI.Models;

namespace MobileAPI.Helpers;

public class RoleSeederService
{
      private readonly IServiceProvider _serviceProvider;

    public RoleSeederService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task SeedRolesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        string[] roleNames = { "Admin" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                // Create the role if it doesn't exist
                await roleManager.CreateAsync(new IdentityRole(roleName));
                Console.WriteLine($"Created role: {roleName}");
            }
        }
    }

    public async Task SeedAdminUserAsync(string adminEmail, string adminPassword)
    {
        using var scope = _serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                FirstName = "admin",
                LastName = "admin",
                Email = adminEmail,
                EmailConfirmed = true
            };
            
            var createUser = await userManager.CreateAsync(adminUser, adminPassword);
            if (createUser.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine("Admin user created successfully");
            }
            else
            {
                Console.WriteLine($"Failed to create admin user: {string.Join(", ", createUser.Errors)}");
            }
        }
        else
        {
            // Ensure admin user has Admin role
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine("Admin role assigned to existing user");
            }
        }
    }
}