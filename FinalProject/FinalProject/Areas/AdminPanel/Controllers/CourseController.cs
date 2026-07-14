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
            // 🎯 DÜZƏLİŞ: .Include(c => c.Teacher) silindi, fənlər müstəqil siyahılanır
            var courses = await _context.courses.ToListAsync();

            return View(courses);
        }

        // ==========================================
        // 2. YENİ FƏNNİN YARADILMASI (GET)
        // ==========================================
        public IActionResult Create()
        {
            // 🎯 DÜZƏLİŞ: Müəllim asılılığı silindiyi üçün müəllim siyahısını yükləməyə ehtiyac qalmadı
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
                // ViewModel-dən gələn təhlükəsiz datanı əsl data modelimizə (Course) köçürürük
                Course newCourse = new Course
                {
                    CourseName = vm.CourseName,
                    CourseCode = vm.CourseCode,
                    Credits = vm.Credits
                    // 🎯 DÜZƏLİŞ: TeacherId mənimsədilməsi silindi
                };

                // Bazaya əlavə edib yadda saxlayırıq
                await _context.courses.AddAsync(newCourse);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Fənn uğurla əlavə edildi!";
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }
    }
}