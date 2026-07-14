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
    [Authorize(Roles = "Admin")] // Yalnız Admin daxil ola bilsin
    public class ScheduleController : Controller
    {
        private readonly CourseDbContext _context;

        public ScheduleController(CourseDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. DƏRS CƏDVƏLİNİN SİYAHISI (INDEX)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // 🎯 DÜZƏLİŞ: Köhnə .ThenInclude(c => c.Teacher) silindi.
            // Müəllimi, fənni və qrupu birbaşa Schedule üzərindən Include edirik.
            var schedulesList = await _context.schedules
                .Include(s => s.Course)
                .Include(s => s.Group)
                .Include(s => s.Teacher) // Müəllimi birbaşa cədvəldən gətiririk
                .OrderBy(s => s.Group.GroupName)
                .ThenBy(s => s.DayOfWeek)
                .ToListAsync();

            return View(schedulesList);
        }

        // ==========================================
        // 2. CƏDVƏLƏ YENİ DƏRS ƏLAVƏ ET (GET)
        // ==========================================
        public async Task<IActionResult> Create()
        {
            var currentCourses = await _context.courses.Where(c => !c.isDeleted).ToListAsync();
            var currentGroups = await _context.Groups.Where(g => !g.isDeleted).ToListAsync();
            var currentTeachers = await _context.teachers.Where(t => !t.isDeleted).ToListAsync(); // 🎯 YENİ: Aktiv müəllimləri çəkirik

            // Seçim qutuları (Dropdown) üçün View-ya ötürürük
            ViewBag.Courses = new SelectList(currentCourses, "Id", "CourseName");
            ViewBag.Groups = new SelectList(currentGroups, "Id", "GroupName");
            ViewBag.Teachers = new SelectList(currentTeachers, "Id", "FullName"); // 🎯 YENİ: Müəllim dropdown-u

            return View();
        }

        // ==========================================
        // 3. CƏDVƏLƏ YENİ DƏRS ƏLAVƏ ET (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateScheduleVM vm)
        {
            // 1. Formdan gələn məlumatların doğruluğunu (Validation) yoxlayırıq
            if (ModelState.IsValid)
            {
                // Məntiqi yoxlama: Başlama saatı bitmə saatından böyük və ya bərabər ola bilməz
                if (vm.StartTime >= vm.EndTime)
                {
                    ModelState.AddModelError("StartTime", "Başlama saatı bitmə saatından sonra ola bilməz.");
                }
                else
                {
                    // 2. ViewModel-dəki dataları əsl Modelimizə köçürürük (Mapping)
                    Schedule newSchedule = new Schedule
                    {
                        CourseId = vm.CourseId,
                        GroupId = vm.GroupId,
                        TeacherId = vm.TeacherId, // 🎯 YENİ: Müəllimi cədvələ bağlayırıq
                        DayOfWeek = vm.DayOfWeek,
                        StartTime = vm.StartTime,
                        EndTime = vm.EndTime,
                        Classroom = vm.Classroom
                    };

                    // 3. Cədvəli verilənlər bazasına yazırıq
                    await _context.schedules.AddAsync(newSchedule);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Dərs cədvələ uğurla əlavə edildi!";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Əgər nəsə xəta varsa, seçim siyahılarını yenidən doldurub səhifəyə qaytarırıq
            var currentCourses = await _context.courses.Where(c => !c.isDeleted).ToListAsync();
            var currentGroups = await _context.Groups.Where(g => !g.isDeleted).ToListAsync();
            var currentTeachers = await _context.teachers.Where(t => !t.isDeleted).ToListAsync(); // 🎯 YENİ

            ViewBag.Courses = new SelectList(currentCourses, "Id", "CourseName", vm.CourseId);
            ViewBag.Groups = new SelectList(currentGroups, "Id", "GroupName", vm.GroupId);
            ViewBag.Teachers = new SelectList(currentTeachers, "Id", "FullName", vm.TeacherId); // 🎯 YENİ

            return View(vm);
        }
    }
}