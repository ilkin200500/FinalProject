using FinalProject.Models;
using FinalProject.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Member> _userManager;
        private readonly SignInManager<Member> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager; // 💡 <int> əlavə etdik

        public AccountController(UserManager<Member> userManager, SignInManager<Member> signInManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
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
                return View(user);
            }

            // 1. İstifadəçini Member sinfi üzərindən tapırıq
            Member existUser = await _userManager.FindByEmailAsync(user.Email);
            if (existUser == null)
            {
                ModelState.AddModelError("", "Belə bir istifadəçi yoxdur və ya məlumatlar yanlışdır.");
                return View(user);
            }

            // 2. İstifadəçinin hesabı aktivdirmi? (Member modelinə əlavə etdiyimiz isActivated işə düşür)
            if (!existUser.isActivated)
            {
                ModelState.AddModelError("", "Hesabınız admin tərəfindən deaktiv edilib. Girişə icazə yoxdur.");
                return View(user);
            }

            // 3. Şifrəni yoxlayıb daxil oluruq
            var signInResult = await _signInManager.PasswordSignInAsync(existUser, user.Password, true, true);

            if (signInResult.Succeeded)
            {
                // Rollara görə yönləndirmə prosesi
                var roles = await _userManager.GetRolesAsync(existUser);

                if (roles.Contains("Admin"))
                {
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

            // Kilidlənmə və digər xətaların idarə olunması
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
            return RedirectToAction("Login", "Account"); // Çıxış edəndən sonra birbaşa Login-ə atsın
        }
    }
}