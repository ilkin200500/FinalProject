using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly CourseDbContext _context;
        private readonly UserManager<Member> _userManager;

        public HomeController(CourseDbContext context, UserManager<Member> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // 🔥 Daxil olan istifadəçinin FullName-ini ViewBag-ə mənimsədirik
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    ViewBag.FullName = currentUser.FullName;
                }
            }

            List<Member> members = _userManager.Users
                .Where(t => !t.isDeleted)
                .ToList();

            return View(members);
        }

        public IActionResult StudentsPage()
        {
            List<Student> students = _context.students.Where(t => !t.isDeleted).ToList();
            return View(students);
        }
    }
}