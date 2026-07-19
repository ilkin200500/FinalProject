using System.Linq;
using System.Threading.Tasks;
using FinalProject.DAL;
using FinalProject.Models;
using FinalProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    [Authorize(Roles = "Admin")]
    public class CourseController : Controller
    {
        private readonly CourseDbContext _context;

        public CourseController(CourseDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. FƏNLƏRİN SİYAHISI (INDEX)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var courses = await _context.courses
                .Include(c => c.Speciality)
                .Where(c => !c.isDeleted)
                .ToListAsync();

            return View(courses);
        }

        // ==========================================
        // 2. YENİ FƏNNİN YARADILMASI (GET)
        // ==========================================
        public async Task<IActionResult> Create()
        {
            ViewBag.Specialities = await _context.specialities.Where(s => !s.isDeleted).ToListAsync();
            return View();
        }

        // ==========================================
        // 3. YENİ FƏNNİN YARADILMASI (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCourseVM vm)
        {
            ModelState.Remove("Speciality");

            if (vm.SpecialityId == 0)
            {
                ModelState.AddModelError("SpecialityId", "Zəhmət olmasa, bir ixtisas seçin!");
            }

            if (ModelState.IsValid)
            {
                Course newCourse = new Course
                {
                    CourseName = vm.CourseName,
                    CourseCode = vm.CourseCode,
                    Credits = vm.Credits,
                    SpecialityId = vm.SpecialityId,
                    isDeleted = false
                };

                await _context.courses.AddAsync(newCourse);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Fənn uğurla əlavə edildi!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Specialities = await _context.specialities.Where(s => !s.isDeleted).ToListAsync();
            return View(vm);
        }
    }
}