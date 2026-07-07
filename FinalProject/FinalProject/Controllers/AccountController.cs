using FinalProject.Models;
using FinalProject.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Controllers
{
    public class AccountController : Controller
    {
       private UserManager<AppUser> _userManager { get; }
        private SignInManager<AppUser> _signInManager { get; }
        private RoleManager<IdentityRole> _roleManager { get; }
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager= userManager;
            _signInManager= signInManager;
            _roleManager= roleManager;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM user)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Nəsə səhv oldu");
                return View(user);
            }

            AppUser existUser = await _userManager.FindByEmailAsync(user.Email);
            if (existUser == null)
            {
                ModelState.AddModelError("", "Belə bir istifadəçi yoxdur");
                return View(user);
            }

            // Giriş cəhdi
            var signInResult = await _signInManager.PasswordSignInAsync(existUser, user.Password, true, true);

            if (signInResult.Succeeded)
            {
                // İstifadəçinin rollarını alırıq
                var roles = await _userManager.GetRolesAsync(existUser);

                // Rollara görə yönləndirmə
                if (roles.Contains("Admin"))
                {
                    // DIQQƏT: Area üçün ən zəmanətli yönləndirmə üsulu budur:
                    return RedirectToRoute(new { area = "AdminPanel", controller = "Dashboard", action = "Index" });
                }
                else if (roles.Contains("Teacher"))
                {
                    return RedirectToAction("Index", "Teacher");
                }
                else if (roles.Contains("Student"))
                {
                    return RedirectToAction("Index", "Student");
                }

                return RedirectToAction("Index", "Home");
            }

            // Əgər giriş uğursuz olubsa
            if (signInResult.IsLockedOut)
            {
                ModelState.AddModelError("", "Hesabınız çoxsaylı uğursuz cəhdə görə müvəqqəti bloklanıb.");
            }
            else
            {
                ModelState.AddModelError("", "E-poçt və ya şifrə yanlışdır.");
            }

            return View(user);
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index","Home");
        }


    }
}
