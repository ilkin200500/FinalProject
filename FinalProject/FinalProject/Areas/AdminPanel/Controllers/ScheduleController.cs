using System;
using System.Collections.Generic;
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
            var schedulesList = await _context.schedules
                .Include(s => s.Course)
                .Include(s => s.Group)
                .Include(s => s.Teacher)
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
            var currentTeachers = await _context.teachers.Where(t => !t.isDeleted).ToListAsync();

            ViewBag.Courses = new SelectList(currentCourses, "Id", "CourseName");
            ViewBag.Groups = new SelectList(currentGroups, "Id", "GroupName");
            ViewBag.Teachers = new SelectList(currentTeachers, "Id", "FullName");

            return View();
        }

        // ==========================================
        // 3. CƏDVƏLƏ YENİ DƏRS ƏLAVƏ ET (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateScheduleVM vm)
        {
            if (ModelState.IsValid)
            {
                // Məntiqi Zaman Yoxlaması: Başlama saatı bitmə saatından böyük və ya bərabər ola bilməz
                if (vm.StartTime >= vm.EndTime)
                {
                    ModelState.AddModelError("StartTime", "Başlama saatı bitmə saatından sonra və ya eyni ola bilməz.");
                }
                else
                {
                    // 🎯 ZİREHLİ YOXALNIŞ 1: İxtisas Uyğunluğu Yoxlanışı
                    var selectedCourse = await _context.courses.FirstOrDefaultAsync(c => c.Id == vm.CourseId);
                    var selectedGroup = await _context.Groups.FirstOrDefaultAsync(g => g.Id == vm.GroupId);

                    if (selectedCourse != null && selectedGroup != null && selectedCourse.SpecialityId != selectedGroup.SpecialityId)
                    {
                        ModelState.AddModelError("CourseId", "Seçilmiş fənn bu qrupun ixtisasına uyğun deyil!");
                    }

                    // 🎯 ZİREHLİ YOXALNIŞ 2: Zaman Çaqqışmalarının Yoxlanılması
                    // Bazadakı cari günə aid olan aktiv bütün cədvəlləri çəkirik
                    var existingSchedules = await _context.schedules
                        .Include(s => s.Teacher)
                        .Include(s => s.Group)
                        .Where(s => s.DayOfWeek == vm.DayOfWeek)
                        .ToListAsync();

                    foreach (var exist in existingSchedules)
                    {
                        // İki saat aralığının kəsişib-kəsişmədiyini yoxlayan riyazi məntiq:
                        // (StartA < EndB) && (EndA > StartB)
                        bool isTimeOverlapping = (vm.StartTime < exist.EndTime) && (vm.EndTime > exist.StartTime);

                        if (isTimeOverlapping)
                        {
                            // A. Müəllim çaqqışması
                            if (exist.TeacherId == vm.TeacherId)
                            {
                                ModelState.AddModelError("TeacherId", $"Bu müəllim həmin saatda ({exist.StartTime:hh\\:mm} - {exist.EndTime:hh\\:mm}) artıq '{exist.Group.GroupName}' qrupuna dərs keçir!");
                                break;
                            }

                            // B. Qrup çaqqışması
                            if (exist.GroupId == vm.GroupId)
                            {
                                ModelState.AddModelError("GroupId", $"Bu qrupun həmin saatda ({exist.StartTime:hh\\:mm} - {exist.EndTime:hh\\:mm}) artıq başqa dərsi var!");
                                break;
                            }

                            // C. Otaq çaqqışması (Boş buraxılmayıbsa və eynidirsə)
                            if (!string.IsNullOrEmpty(vm.Classroom) && exist.Classroom == vm.Classroom)
                            {
                                ModelState.AddModelError("Classroom", $"'{vm.Classroom}' auditoriyası həmin saatda artıq '{exist.Group.GroupName}' qrupu tərəfindən zəbt edilib!");
                                break;
                            }
                        }
                    }

                    // Əgər yuxarıdakı yoxlanışların heç birindən xəta (ModelState Error) gəlmədisə, bazaya yazırıq
                    if (ModelState.IsValid)
                    {
                        Schedule newSchedule = new Schedule
                        {
                            CourseId = vm.CourseId,
                            GroupId = vm.GroupId,
                            TeacherId = vm.TeacherId,
                            DayOfWeek = vm.DayOfWeek,
                            StartTime = vm.StartTime,
                            EndTime = vm.EndTime,
                            Classroom = vm.Classroom
                        };

                        await _context.schedules.AddAsync(newSchedule);
                        await _context.SaveChangesAsync();

                        TempData["Success"] = "Dərs cədvələ uğurla və təhlükəsiz şəkildə əlavə edildi!";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            // Xəta olduqda dropdown-ları yenidən doldurub səhifəyə qaytarırıq
            var currentCourses = await _context.courses.Where(c => !c.isDeleted).ToListAsync();
            var currentGroups = await _context.Groups.Where(g => !g.isDeleted).ToListAsync();
            var currentTeachers = await _context.teachers.Where(t => !t.isDeleted).ToListAsync();

            ViewBag.Courses = new SelectList(currentCourses, "Id", "CourseName", vm.CourseId);
            ViewBag.Groups = new SelectList(currentGroups, "Id", "GroupName", vm.GroupId);
            ViewBag.Teachers = new SelectList(currentTeachers, "Id", "FullName", vm.TeacherId);

            return View(vm);
        }
    }
}