using Microsoft.AspNetCore.Identity;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;

namespace Sym_Connect__Capstone2_.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                serviceProvider.GetRequiredService<UserManager<User>>();

            var context =
                serviceProvider.GetRequiredService<AppDbContext>();

            await SeedRolesAsync(roleManager);

            await SeedUserAsync(
                userManager,
                context,
                email: "SYMadmin@gmail.com",
                password: "Admin@123",
                firstName: "Admin",
                lastName: "Administrator",
                photoUrl: "https://res.cloudinary.com/dxzmz62w4/image/upload/v1787378932/ChatGPT_Image_Aug_22_2026_02_05_02_PM_j5vans.png",
                role: "Admin"
            );

            await SeedUserAsync(
                userManager,
                context,
                email: "SYMleader@gmail.com",
                password: "Leader@123",
                firstName: "Leader",
                lastName: "User",
                photoUrl: "https://res.cloudinary.com/dxzmz62w4/image/upload/v1787378932/ChatGPT_Image_Aug_22_2026_02_05_02_PM_j5vans.png",
                role: "Leader"
            );

            await SeedUserAsync(
                userManager,
                context,
                email: "herschelle@gmail.com",
                password: "hers123456",
                firstName: "Hers",
                lastName: "User",
                photoUrl: "https://res.cloudinary.com/dxzmz62w4/image/upload/v1787378932/ChatGPT_Image_Aug_22_2026_02_05_02_PM_j5vans.png",
                role: "Member"
            );

            await SeedUserAsync(
                userManager,
                context,
                email: "Jason@gmail.com",
                password: "jason123456",
                firstName: "Jason",
                lastName: "User",
                photoUrl: "https://res.cloudinary.com/dxzmz62w4/image/upload/v1787378932/ChatGPT_Image_Aug_22_2026_02_05_02_PM_j5vans.png",
                role: "Member"
            );

            await SeedUserAsync(
                userManager,
                context,
                email: "windsay@gmail.com",
                password: "jason123456",
                firstName: "Windsay",
                lastName: "User",
                photoUrl: "https://res.cloudinary.com/dxzmz62w4/image/upload/v1787378932/ChatGPT_Image_Aug_22_2026_02_05_02_PM_j5vans.png",
                role: "Member"
            );
        }

        private static async Task SeedRolesAsync(
            RoleManager<IdentityRole> roleManager)
        {
            string[] roles =
            {
                "Admin",
                "Leader",
                "Member"
            };

            foreach (var role in roles)
            {
                bool roleExists =
                    await roleManager.RoleExistsAsync(role);

                if (!roleExists)
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role)
                    );
                }
            }
        }

        private static async Task SeedUserAsync(
            UserManager<User> userManager,
            AppDbContext context,
            string email,
            string password,
            string firstName,
            string lastName,
            string photoUrl,
            string role,
            string gender = "Unspecified",
            string contactNumber = "0000000000")
        {
            var existingUser =
                await userManager.FindByEmailAsync(email);

            if (existingUser != null)
                return; // already seeded

            // Member must exist FIRST so we have a MemberId to hand to the Identity user.
            var member = new Member
            {
                FirstName = firstName,
                LastName = lastName,
                ContactNumber = contactNumber,
                Gender = gender,
                Status = "Active",
                ProfilePictureUrl = photoUrl,
                DateRegistered = DateTime.Now
            };

            context.Members.Add(member);
            await context.SaveChangesAsync(); // populates member.MemberId

            var user = new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                ProfilePictureUrl = photoUrl,
                DisplayName = $"{firstName} {lastName}",
                MemberId = member.MemberId
            };

            var result =
                await userManager.CreateAsync(
                    user,
                    password
                );

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(
                    user,
                    role
                );

                // complete the reverse link
                member.UserId = user.Id;
                await context.SaveChangesAsync();
            }
            else
            {
                // Identity creation failed - don't leave an orphaned Member row behind
                context.Members.Remove(member);
                await context.SaveChangesAsync();
            }
        }
    }
}