using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly CourseDbContext _context;
        private readonly UserManager<Member> _userManager;

        public StudentController(CourseDbContext context, UserManager<Member> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return NotFound();

            var student = _context.students.FirstOrDefault(s => s.MemberId == currentMember.Id);
            if (student == null) return View("Error");

            // Cədvəl adını kiçik hərflə 'grades' qoyduğumuz üçün bura tam uyğundur
            var grades = _context.grades
                .Include(g => g.Course)
                .Where(g => g.StudentId == student.Id)
                .ToList();

            ViewBag.StudentName = student.FullName;

            return View(grades);
        }
    }
}