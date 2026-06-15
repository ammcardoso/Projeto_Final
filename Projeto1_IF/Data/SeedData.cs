using Microsoft.AspNetCore.Identity;
using Projeto1_IF.Models;

namespace Projeto1_IF.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<db_IFContext>();

            string[] roles = new[] { "GerenteMedico", "GerenteNutricionista", "GerenteGeral" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create three manager users if they don't exist (use .com domains for easier login)
            await CreateManagerUser(userManager, "gerentemedico@local.com", "gerentemedico@local.com", "GerenteMedico");
            await CreateManagerUser(userManager, "gerentenutricionista@local.com", "gerentenutricionista@local.com", "GerenteNutricionista");
            await CreateManagerUser(userManager, "gerentegeral@local.com", "gerentegeral@local.com", "GerenteGeral");
        }

        private static async Task CreateManagerUser(UserManager<IdentityUser> userManager, string email, string userName, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser { UserName = userName, Email = email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, "P@ssw0rd!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
