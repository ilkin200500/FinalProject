using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly CourseDbContext _context;
        private readonly UserManager<Member> _userManager;

        public TeacherController(CourseDbContext context, UserManager<Member> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // 1. MÜƏLLİMİN ÖZ FƏNLƏRİNİN SİYAHISI
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var teacher = await _context.teachers.FirstOrDefaultAsync(t => t.MemberId == currentMember.Id);
            if (teacher == null) return View("Error");

            var myCourses = await _context.schedules
                .Include(s => s.Course)
                .Where(s => s.TeacherId == teacher.Id && s.Course != null)
                .Select(s => s.Course)
                .Distinct()
                .ToListAsync();

            return View(myCourses);
        }

        // ==========================================
        // 2. SEÇİLMİŞ FƏNN ÜZRƏ TƏLƏBƏLƏRİN SİYAHISI (DÜZƏLDİLDİ 🎯)
        // ==========================================
        public async Task<IActionResult> StudentsReportCard(int courseId)
        {
            var course = await _context.courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null) return NotFound();

            ViewBag.CourseId = courseId;
            ViewBag.CourseName = course.CourseName;

            // Səhvə səbəb olan .ThenInclude(s => s.Member) hissəsi tamamilə silindi.
            // Çünki tələbənin FullName sahəsi elə Student modelinin özündədir.
            var registeredStudents = await _context.courseRegistrations
                .Include(cr => cr.Student)
                .Where(cr => cr.CourseId == courseId && cr.Student != null)
                .Select(cr => cr.Student)
                .ToListAsync();

            var currentGrades = await _context.grades
                .Where(g => g.CourseId == courseId)
                .ToListAsync();

            var jurnals = registeredStudents.Select(student => {
                var grade = currentGrades.FirstOrDefault(g => g.StudentId == student.Id);
                return new Grade
                {
                    Id = grade?.Id ?? 0,
                    StudentId = student.Id,
                    Student = student,
                    CourseId = courseId,
                    Mids = grade?.Mids ?? 0,
                    Final = grade?.Final,
                    Semester = grade?.Semester ?? "2026 Yaz"
                };
            }).ToList();

            return View(jurnals);
        }

        // ==========================================
        // 3. QİYMƏT DAXİL ETMƏK VƏ YA YENİLƏMƏK
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignGrade(int studentId, int courseId, int mids, int? final, string semester = "2026 Yaz")
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var teacher = await _context.teachers.FirstOrDefaultAsync(t => t.MemberId == currentMember.Id);
            if (teacher == null)
            {
                TempData["Error"] = "Sistemdə müəllim profiliniz tapılmadı!";
                return RedirectToAction(nameof(StudentsReportCard), new { courseId = courseId });
            }

            if (mids < 0 || mids > 50 || (final.HasValue && (final.Value < 0 || final.Value > 50)))
            {
                TempData["Error"] = "Giriş balı və Final balı 0 ilə 50 arasında olmalıdır!";
                return RedirectToAction(nameof(StudentsReportCard), new { courseId = courseId });
            }

            var existingGrade = await _context.grades
                .FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseId == courseId);

            if (existingGrade != null)
            {
                existingGrade.Mids = mids;
                existingGrade.Final = final;
                existingGrade.Semester = semester;
                existingGrade.TeacherId = teacher.Id;

                _context.Update(existingGrade);
                TempData["Success"] = "Ballar uğurla yeniləndi.";
            }
            else
            {
                var newGrade = new Grade
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    TeacherId = teacher.Id,
                    Mids = mids,
                    Final = final,
                    Semester = semester,
                    CreatedAt = DateTime.Now
                };

                await _context.AddAsync(newGrade);
                TempData["Success"] = "İlk qiymət uğurla daxil edildi.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(StudentsReportCard), new { courseId = courseId });
        }
    }
}