using System.Threading.Tasks;
using FinalProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Controllers
{
    public class RoleController : Controller
    {
        private RoleManager<IdentityRole> _roleManager { get; }
        private UserManager<AppUser> _userManager { get; }
        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            
        }
        public async Task<IActionResult> CreateRoles()
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
            await _roleManager.CreateAsync(new IdentityRole("Telebe"));
            await _roleManager.CreateAsync(new IdentityRole("Muellim"));
            return Content("rollar yaranduiui");
        }

        public async Task<IActionResult> CreateFirstAdmin()
        {
            // 1. Öncə rolları yaradırıq (əgər yoxdursa)
            string[] roleNames = { "Admin", "Teacher", "Student" };
            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. İlk Admin istifadəçisini yaradırıq
            string adminEmail = "admin@gmail.com";
            var existUser = await _userManager.FindByEmailAsync(adminEmail);

            if (existUser == null)
            {
                AppUser adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true, // Identity-nin e-poçt təsdiqi tələbini keçmək üçün
                    FullName = "Sistem Admin"
                };

                // İstifadəçini şifrəsi ilə birlikdə yaradırıq
                var result = await _userManager.CreateAsync(adminUser, "Admin123!");

                if (result.Succeeded)
                {
                    // Yaradılan bu istifadəçiyə "Admin" rolunu yapışdırırıq
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    return Content("Uğurlu! Rollar yaradıldı və ilk Admin hesabı quruldu. Email: admin@sis.com , Şifrə: Admin123!");
                }
                else
                {
                    return Content("İstifadəçi yaradılanda xəta baş verdi: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            return Content("Admin istifadəçisi artıq bazada mövcuddur.");
        }
    }
}
