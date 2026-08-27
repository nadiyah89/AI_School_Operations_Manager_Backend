using Microsoft.AspNetCore.Identity;

namespace SchoolOperations.Data
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            // Get RoleManager from dependency injection
            var roleManager = serviceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            // Roles required by the school operations system
            string[] roles =
            {
                "Admin",
                "Teacher",
                "Student",
                "Parent"
            };

            // Check each role
            foreach (var role in roles)
            {
                // If the role does not exist, create it
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }
        }
    }
}