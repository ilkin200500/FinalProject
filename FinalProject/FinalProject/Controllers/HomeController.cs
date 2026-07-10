using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Identity; // 💡 Bu mütləq əlavə olunmalıdır
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace FinalProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly CourseDbContext _context;
        private readonly UserManager<Member> _userManager; // 💡 Identity istifadəçilərini idarə etmək üçün əlavə etdik

        // Constructor daxilində həm DbContext-i, həm də UserManager-i qəbul edirik
        public HomeController(CourseDbContext context, UserManager<Member> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            // 💡 DÜZƏLİŞ: _context.members əvəzinə _userManager.Users istifadə edirik:
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