using FinalProject.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace FinalProject.DAL
{
    public static class DbInitializer
    {
        public static async Task SeedAdminAsync(UserManager<Member> userManager, RoleManager<IdentityRole> roleManager)
        {
            // 1. Rolların yoxlanılması və yaradılması
            string[] roleNames = { "Admin", "Student", "Teacher" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Admin istifadəçisinin yoxlanılması
            string adminEmail = "admin@finalproject.com";
            string adminUsername = "admin2026";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // Yeni Admin obyekti yaradırıq
                var admin = new Member
                {
                    UserName = adminUsername,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    // Sənin Member modelində FullName varsa, bura əlavə et:
                    // FullName = "Sistem Administratoru"
                };

                // Admini şifrəsi ilə bərabər bazaya daxil edirik (Avtomatik Hash olunur)
                var createAdmin = await userManager.CreateAsync(admin, "Admin123!");

                if (createAdmin.Succeeded)
                {
                    // Yaradılan istifadəçiyə Admin rolunu təyin edirik
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}