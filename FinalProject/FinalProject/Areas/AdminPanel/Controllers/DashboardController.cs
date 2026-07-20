using FinalProject.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FinalProject.Areas.AdminPanel.Controllers
{
    // [Authorize(Roles ="Admin")] // Hər şey tam hazır olduqdan sonra bu sətri aktivləşdirə bilərsən
    [Area("AdminPanel")]
    public class DashboardController : Controller
    {
        private readonly CourseDbContext _context;

        // Verilənlər bazası kontekstini (Context) buraya daxil edirik
        public DashboardController(CourseDbContext context)
        {
            _context = context;
        }

        // Sayları asinxron olaraq çəkmək üçün metodu "public async Task<IActionResult>" etdik
        public async Task<IActionResult> Index()
        {
            // Bazadakı ümumi sayları dinamik olaraq hesablayıb ViewBag-ə doldururuq
            ViewBag.TotalStudents = await _context.students.CountAsync();
            ViewBag.TotalTeachers = await _context.teachers.CountAsync();
            ViewBag.TotalGroups = await _context.Groups.CountAsync();
            ViewBag.TotalCourses = await _context.courses.CountAsync();

            return View();
        }
    }
}