using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        // ==========================================
        // 1. TƏLƏBƏNİN SEMESTR QİYMƏTLƏRİ (INDEX)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return NotFound();

            var student = await _context.students.FirstOrDefaultAsync(s => s.MemberId == currentMember.Id);
            if (student == null) return View("Error");

            var grades = await _context.grades
                .Include(g => g.Course)
                .Where(g => g.StudentId == student.Id)
                .ToListAsync();

            ViewBag.StudentName = student.FullName;
            return View(grades);
        }

        // ==========================================
        // 2. TƏLƏBƏ ÜÇÜN SEÇƏ BİLƏCƏYİ FƏNLƏRİN SİYAHISI
        // ==========================================
        public async Task<IActionResult> AvailableCourses()
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var student = await _context.students.FirstOrDefaultAsync(s => s.MemberId == currentMember.Id);
            if (student == null) return NotFound("Tələbə profili tapılmadı.");

            // Tələbənin artıq qeydiyyatdan keçdiyi fənlərin ID-lərini alırıq
            var enrolledCourseIds = await _context.courseRegistrations
                .Where(cr => cr.StudentId == student.Id)
                .Select(cr => cr.CourseId)
                .ToListAsync();

            // Cari tələbənin qrupuna aid və hələ seçmədiyi fənləri birbaşa gətiririk
            var availableSchedules = await _context.schedules
                .Include(s => s.Course)
                .Where(s => s.GroupId == student.GroupId && !enrolledCourseIds.Contains(s.CourseId))
                .ToListAsync();

            // Eyni fənnin fərqli günlərdəki cədvəllərinin təkrarlanmasının qarşısını alırıq
            var uniqueSchedules = availableSchedules
                .GroupBy(s => s.CourseId)
                .Select(g => g.First())
                .ToList();

            ViewBag.StudentName = student.FullName;

            return View(uniqueSchedules);
        }

        // ==========================================
        // 3. FƏNNİ GÖTÜR (ENROLL) POST METODU
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollCourse(int courseId)
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var student = await _context.students.FirstOrDefaultAsync(s => s.MemberId == currentMember.Id);
            if (student == null) return NotFound();

            var alreadyEnrolled = await _context.courseRegistrations
                .AnyAsync(cr => cr.StudentId == student.Id && cr.CourseId == courseId);

            if (alreadyEnrolled)
            {
                TempData["Error"] = "Siz bu fənnə artıq qeydiyyatdan keçmisiniz!";
                return RedirectToAction(nameof(AvailableCourses));
            }

            var schedule = await _context.schedules
                .FirstOrDefaultAsync(s => s.CourseId == courseId && s.GroupId == student.GroupId);

            if (schedule == null)
            {
                TempData["Error"] = "Bu fənn üçün aktiv dərs cədvəli tapılmadı!";
                return RedirectToAction(nameof(AvailableCourses));
            }

            var registration = new CourseRegistration
            {
                StudentId = student.Id,
                CourseId = courseId
            };

            var grade = new Grade
            {
                StudentId = student.Id,
                CourseId = courseId,
                Mids = 0,
                Final = null,
                TeacherId = schedule.TeacherId, // Qiymətləndirmə üçün arxa planda müəllim ID-si hələ də qeyd olunur
                Semester = "2026 Yaz",
                CreatedAt = DateTime.Now
            };

            await _context.courseRegistrations.AddAsync(registration);
            await _context.grades.AddAsync(grade);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Fənnə uğurla qeydiyyatdan keçdiniz!";
            return RedirectToAction(nameof(AvailableCourses));
        }

        // ==========================================
        // 4. TƏLƏBƏNİN QEYDİYYATDAN KEÇDİYİ FƏNLƏR (MY COURSES)
        // ==========================================
        public async Task<IActionResult> MyCourses()
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var student = await _context.students.FirstOrDefaultAsync(s => s.MemberId == currentMember.Id);
            if (student == null) return NotFound("Tələbə profili tapılmadı.");

            var registeredCourses = await _context.courseRegistrations
                .Include(cr => cr.Course)
                .Where(cr => cr.StudentId == student.Id)
                .Select(cr => cr.Course)
                .ToListAsync();

            ViewBag.StudentName = student.FullName;
            return View(registeredCourses);
        }

        // ==========================================
        // 5. TƏLƏBƏNİN HƏFTƏLİK DƏRS CƏDVƏLİ (SCHEDULES)
        // ==========================================
        public async Task<IActionResult> Schedules()
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var student = await _context.students
                .Include(s => s.Group)
                .FirstOrDefaultAsync(s => s.MemberId == currentMember.Id);

            if (student == null) return NotFound("Tələbə profili tapılmadı.");

            var schedules = await _context.schedules
                .Include(s => s.Course)
                .Include(s => s.Teacher)
                .Where(s => s.GroupId == student.GroupId)
                .ToListAsync();

            ViewBag.StudentName = student.FullName;
            ViewBag.GroupName = student.Group != null ? student.Group.GroupName : "Qrup təyin edilməyib";

            return View(schedules);
        }

        // =======================================================
        // 🔔 6. TƏLƏBƏNİN BÜTÜN BİLDİRİŞLƏR SƏHİFƏSİ (YENİ)
        // =======================================================
        public async Task<IActionResult> Notifications()
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var student = await _context.students.FirstOrDefaultAsync(s => s.MemberId == currentMember.Id);
            if (student == null) return NotFound("Tələbə profili tapılmadı.");

            // Tələbənin bütün bildirişlərini tarixinə görə azalan sıra ilə çəkirik
            var notificationsList = await _context.notifications
                .Where(n => n.StudentId == student.Id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            // Tələbə bu səhifəyə daxil olanda, oxunmamış olan bütün bildirişləri "Oxundu" edirik
            var unreadNotifications = notificationsList.Where(n => !n.IsRead).ToList();
            if (unreadNotifications.Any())
            {
                foreach (var item in unreadNotifications)
                {
                    item.IsRead = true;
                }

                // Bazada dəyişiklikləri qeyd edirik (kiçik "notifications" DbSet-i ilə)
                await _context.SaveChangesAsync();
            }

            ViewBag.StudentName = student.FullName;
            return View(notificationsList);
        }
    }
}