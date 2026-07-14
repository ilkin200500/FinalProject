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
        // 1. TƏLƏBƏNİN SEMESTR QİYMƏTLƏRİ
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
        // 2. TƏLƏBƏ ÜÇÜN SEÇƏ BİLƏCƏYİ FƏNLƏRİN SİYAHISI (TAM DÜZƏLDİLDİ 🎯)
        // ==========================================
        public async Task<IActionResult> AvailableCourses()
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var student = await _context.students.FirstOrDefaultAsync(s => s.MemberId == currentMember.Id);
            if (student == null) return NotFound("Tələbə profili tapılmadı.");

            // 1. Tələbənin artıq qeydiyyatdan keçdiyi fənlərin ID-lərini alırıq
            var enrolledCourseIds = await _context.courseRegistrations
                .Where(cr => cr.StudentId == student.Id)
                .Select(cr => cr.CourseId)
                .ToListAsync();

            // 2. Tələbənin qrupuna aid olan dərs cədvəllərindən fənləri və müəllimləri birlikdə çəkirik
            var availableCourses = await _context.schedules
                .Include(s => s.Course)     // Fənni gətiririk
                .Include(s => s.Teacher)    // Müəllimi gətiririk
                .Where(s => s.GroupId == student.GroupId && !enrolledCourseIds.Contains(s.CourseId))
                .Select(s => s.Course)      // View-nun List<Course> gözləməsi ehtimalına qarşı Course obyektini seçirik
                .Distinct()
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

            var alreadyEnrolled = await _context.courseRegistrations
                .AnyAsync(cr => cr.StudentId == student.Id && cr.CourseId == courseId);

            if (alreadyEnrolled)
            {
                TempData["Error"] = "Siz bu fənnə artıq qeydiyyatdan keçmisiniz!";
                return RedirectToAction(nameof(AvailableCourses));
            }

            // Həmin fənni tədris edən müəllimi tapmaq üçün dərs cədvəlindən (Schedules) məlumatı çəkirik.
            var schedule = await _context.schedules
                .FirstOrDefaultAsync(s => s.CourseId == courseId && s.GroupId == student.GroupId);

            if (schedule == null)
            {
                TempData["Error"] = "Bu fənn üçün aktiv dərs cədvəli və ya müəllim tapılmadı!";
                return RedirectToAction(nameof(AvailableCourses));
            }

            // Yeni qeydiyyat obyekti yaradılır
            var registration = new CourseRegistration
            {
                StudentId = student.Id,
                CourseId = courseId
            };

            // Qiymətlər cədvəlində boş sətir açırıq
            var grade = new Grade
            {
                StudentId = student.Id,
                CourseId = courseId,
                Mids = 0,               // Giriş balı ilkin olaraq 0 təyin edilir
                Final = null,           // Final imtahanı hələ olmadığı üçün null qalır
                TeacherId = schedule.TeacherId, // Dərsi keçən müəllim cədvəldən avtomatik götürülür
                Semester = "2026 Yaz",  // Cari semestr 
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
    }
}