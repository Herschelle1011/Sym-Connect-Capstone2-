using Microsoft.AspNetCore.Identity;
using Sym_Connect__Capstone2_.Models;

namespace Sym_Connect__Capstone2_.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            await SeedRolesAsync(roleManager);

            await SeedUserAsync(
                userManager,
                email: "SYMadmin@gmail.com",
                password: "Admin@123",
                firstName: "Admin",
                lastName: "Administrator",
                role: "Admin"
            );

            await SeedUserAsync(
                userManager,
                email: "SYMleader@gmail.com",
                password: "Leader@123",
                firstName: "Leader",
                lastName: "User",
                role: "Leader"
            );
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Leader", "Member" };

            foreach (var role in roles)
            {
                bool roleExists = await roleManager.RoleExistsAsync(role);

                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedUserAsync(
            UserManager<User> userManager,
            string email,
            string password,
            string firstName,
            string lastName,
            string role)
        {
            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser == null)
            {
                var user = new User
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}