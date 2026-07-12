using System.Linq;
using System.Threading.Tasks;
using FinalProject.DAL;
using FinalProject.Models;
using FinalProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    [Authorize(Roles = "Admin")] // Yalnız Admin rolunda olanlar daxil ola bilər
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
            // Fənləri və onlara təyin olunmuş müəllimləri bazadan çəkirik
            var courses = await _context.courses
                .Include(c => c.Teacher)
                .ToListAsync();

            return View(courses);
        }

        // ==========================================
        // 2. YENİ FƏNNİN YARADILMASI (GET)
        // ==========================================
        public async Task<IActionResult> Create()
        {
            // Fənnə müəllim təyin edə bilmək üçün müəllimlərin siyahısını götürürük
            var teachers = await _context.teachers.ToListAsync();

            // Dropdown (seçim qutusu) üçün müəllim siyahısını ViewBag-ə qoyuruq
            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName");

            return View();
        }

        // ==========================================
        // 3. YENİ FƏNNİN YARADILMASI (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCourseVM vm)
        {
            // Gələn ViewModel-in doğruluğunu (Validation) yoxlayırıq
            if (ModelState.IsValid)
            {
                // ViewModel-dən gələn təhlükəsiz datanı əsl data modelimizə (Course) köçürürük (Mapping)
                Course newCourse = new Course
                {
                    CourseName = vm.CourseName,
                    CourseCode = vm.CourseCode,
                    Credits = vm.Credits,
                    TeacherId = vm.TeacherId
                };

                // Bazaya əlavə edib yadda saxlayırıq
                await _context.courses.AddAsync(newCourse);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Fənn uğurla əlavə edildi və müəllim təyin olundu!";
                return RedirectToAction(nameof(Index));
            }

            // Əgər formda xəta varsa (məs: kredit səhv yazılıb), müəllimləri yenidən doldurub səhifəyə qaytarırıq
            var teachers = await _context.teachers.ToListAsync();
            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName", vm.TeacherId);

            return View(vm);
        }
    }
}