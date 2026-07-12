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

            var myCourses = await _context.courses
                .Where(c => c.TeacherId == teacher.Id)
                .ToListAsync();

            return View(myCourses);
        }

        // ==========================================
        // 2. SEÇİLMİŞ FƏNN ÜZRƏ TƏLƏBƏLƏRİN SİYAHISI (BDU Portalı Məntiqi)
        // ==========================================
        public async Task<IActionResult> StudentsReportCard(int courseId)
        {
            var course = await _context.courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null) return NotFound();

            ViewBag.CourseId = courseId;
            ViewBag.CourseName = course.CourseName;

            // 🎯 Addım A: Bu fənni 'courseRegistrations' cədvəlindən qeydiyyatdan keçirmiş aktiv tələbələri tapırıq
            var registeredStudents = await _context.courseRegistrations
                .Include(cr => cr.Student)
                .Where(cr => cr.CourseId == courseId)
                .Select(cr => cr.Student)
                .ToListAsync();

            // Addım B: Bu fənnə aid artıq yazılmış mövcud qiymətləri bazadan çəkirik
            var currentGrades = await _context.grades
                .Where(g => g.CourseId == courseId)
                .ToListAsync();

            // Addım C: Qeydiyyatda olan hər bir tələbə üçün qiymət obyektini sinxronlaşdırırıq
            var jurnals = registeredStudents.Select(student => {
                var grade = currentGrades.FirstOrDefault(g => g.StudentId == student.Id);
                return new Grade
                {
                    StudentId = student.Id,
                    Student = student,
                    CourseId = courseId,
                    Mids = grade?.Mids ?? 0, // Qiymət hələ yoxdursa ekranda 0 göstər
                    Final = grade?.Final     // Final balı (null ola bilər, model özü hesablayacaq)
                };
            }).ToList();

            return View(jurnals);
        }

        // ==========================================
        // 3. QİYMƏT DAXİL ETMƏK VƏ YA YENİLƏMƏK
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignGrade(int studentId, int courseId, int mids, int? final, string semester = "2026 Payız")
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var teacher = await _context.teachers.FirstOrDefaultAsync(t => t.MemberId == currentMember.Id);
            if (teacher == null) return NotFound();

            // Qiymət aralığı yoxlanışı (0-50 bal limiti)
            if (mids < 0 || mids > 50 || (final.HasValue && (final.Value < 0 || final.Value > 50)))
            {
                TempData["Error"] = "Giriş balı və Final balı 0 ilə 50 arasında olmalıdır!";
                return RedirectToAction(nameof(StudentsReportCard), new { courseId = courseId });
            }

            // `grades` cədvəlində bu tələbəyə aid sətir varmı deyə yoxlayırıq
            var existingGrade = await _context.grades
                .FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseId == courseId);

            if (existingGrade != null)
            {
                // 🔄 Sətir varsa: Sadəcə balları və müəllim məlumatını yeniləyirik
                existingGrade.Mids = mids;
                existingGrade.Final = final;
                existingGrade.Semester = semester;
                existingGrade.TeacherId = teacher.Id;

                _context.Update(existingGrade);
                TempData["Success"] = "Ballar uğurla yeniləndi.";
            }
            else
            {
                // ➕ Sətir yoxdursa (İlk dəfə qiymət yazılırsa): Yeni bir qiymət sətiri yaradırıq
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