using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Models;
    
namespace Server.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // 1. Clean up duplicates before applying the Unique Index migration
            await CleanUpDuplicateEmails(context);

            // 2. Ensure database is created and migrations are applied
            await context.Database.MigrateAsync();

            await SeedRolesAsync(context);
            await SeedUsersAsync(context);
        }

        private static async Task CleanUpDuplicateEmails(AppDbContext context)
        {
            // If the table doesn't exist yet (first run), this might fail, so we check
            if (!await context.Database.CanConnectAsync()) return;

            var duplicates = await context.Users
                .GroupBy(u => u.UserEmail)
                .Where(g => g.Count() > 1)
                .Select(g => new { Email = g.Key, KeepId = g.Min(u => u.UserId) })
                .ToListAsync();

            if (duplicates.Any())
            {
                foreach (var dup in duplicates)
                {
                    var toDelete = await context.Users
                        .Where(u => u.UserEmail == dup.Email && u.UserId != dup.KeepId)
                        .ToListAsync();
                    
                    context.Users.RemoveRange(toDelete);
                }
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedRolesAsync(AppDbContext context)
        {
            if (await context.Roles.AnyAsync()) return;

            var roles = new List<Role>
            {
                new Role { RoleName = "Admin" },
                new Role { RoleName = "Developer" },
                new Role { RoleName = "Tester" },
                new Role { RoleName = "Manager" }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        private static async Task SeedUsersAsync(AppDbContext context)
        {
            if (await context.Users.AnyAsync()) return;

            var passwordHasher = new PasswordHasher<User>();
            
            // Define Users
            var adminUser = new User { Username = "admin", UserEmail = "admin@example.com" };
            var dev1User = new User { Username = "dev1", UserEmail = "dev1@example.com" };
            var dev2User = new User { Username = "dev2", UserEmail = "dev2@example.com" };
            var testerUser = new User { Username = "tester", UserEmail = "tester@example.com" };
            var managerUser = new User { Username = "manager", UserEmail = "manager@example.com" };

            var users = new List<User> { adminUser, dev1User, dev2User, testerUser, managerUser };

            // Hash Passwords
            foreach (var user in users)
            {
                user.HashPassword = passwordHasher.HashPassword(user, "Password123!");
            }

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            // Assign Roles
            var roles = await context.Roles.ToListAsync();
            var adminRole = roles.First(r => r.RoleName == "Admin");
            var devRole = roles.First(r => r.RoleName == "Developer");
            var testerRole = roles.First(r => r.RoleName == "Tester");
            var managerRole = roles.First(r => r.RoleName == "Manager");

            var userRoles = new List<UserRole>
            {
                new UserRole { UserId = adminUser.UserId, RoleId = adminRole.RoleId },
                new UserRole { UserId = dev1User.UserId, RoleId = devRole.RoleId },
                new UserRole { UserId = dev2User.UserId, RoleId = devRole.RoleId },
                new UserRole { UserId = testerUser.UserId, RoleId = testerRole.RoleId },
                new UserRole { UserId = managerUser.UserId, RoleId = managerRole.RoleId }
            };

            await context.UserRoles.AddRangeAsync(userRoles);
            await context.SaveChangesAsync();
        }
    }
}
