using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
        // 1. TƏLƏBƏNİN SEMESTR QİYMƏTLƏRİ (MÖVCUD İNDEX)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return NotFound();

            var student = await _context.students.FirstOrDefaultAsync(s => s.MemberId == currentMember.Id);
            if (student == null) return View("Error");

            // Daha sürətli asinxron (ToListAsync) sorğuya keçirdik
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

            // Tələbənin cari semestrdə artıq seçmiş olduğu fənlərin ID-lərini tapırıq
            var enrolledCourseIds = await _context.courseRegistrations
                .Where(cr => cr.StudentId == student.Id)
                .Select(cr => cr.CourseId)
                .ToListAsync();

            // Tələbəyə yalnız hələ seçmədiyi (yeni) fənləri siyahılayırıq
            var availableCourses = await _context.courses
                .Include(c => c.Teacher) // Müəllim adını View-da göstərmək üçün
                .Where(c => !enrolledCourseIds.Contains(c.Id))
                .ToListAsync();

            ViewBag.StudentName = student.FullName;
            return View(availableCourses);
        }

        // ==========================================
        // 3. FƏNNİ GÖTÜR (ENROLL) DÜYMƏSİNİN POST METODU
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollCourse(int courseId)
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var student = await _context.students.FirstOrDefaultAsync(s => s.MemberId == currentMember.Id);
            if (student == null) return NotFound();

            // Təkrar qeydiyyatın qarşısını tam almaq üçün yoxlanış
            var alreadyEnrolled = await _context.courseRegistrations
                .AnyAsync(cr => cr.StudentId == student.Id && cr.CourseId == courseId);

            if (alreadyEnrolled)
            {
                TempData["Error"] = "Siz bu fənnə artıq qeydiyyatdan keçmisiniz!";
                return RedirectToAction(nameof(AvailableCourses));
            }

            // BDU Portalı Məntiqi: 'courseRegistrations' cədvəlinə yeni sətir yazılır
            var registration = new CourseRegistration
            {
                StudentId = student.Id,
                CourseId = courseId,
                Semester = "2026 Payız",
                RegisteredAt = DateTime.Now
            };

            await _context.courseRegistrations.AddAsync(registration);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Fənn uğurla seçildi! Müəllimin jurnalına əlavə olundunuz.";
            return RedirectToAction(nameof(AvailableCourses));
        }
    }
}