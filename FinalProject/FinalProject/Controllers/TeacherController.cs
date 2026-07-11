using System;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly CourseDbContext _context;

        // Konstruktordakı təyinat düzəldildi: _context = context;
        public TeacherController(CourseDbContext context)
        {
            _context = context; // 🎯 Doğru yazılış budur
        }

        // Müəllimin tədris etdiyi fənlərin siyahısı
        public async Task<IActionResult> Index()
        {
            var courses = await _context.courses.ToListAsync();
            return View(courses);
        }

        // Seçilmiş fənn üzrə tələbələrin və onların mövcud ballarının siyahısı
        public async Task<IActionResult> StudentsReportCard(int courseId)
        {
            var course = await _context.courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null) return NotFound();

            ViewBag.CourseId = courseId;
            ViewBag.CourseName = course.CourseName;

            var grades = await _context.grades
                .Include(g => g.Student)
                .Where(g => g.CourseId == courseId)
                .ToListAsync();

            return View(grades);
        }

        // Qiymət daxil etmək və ya Yeniləmək üçün POST Action-ı
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignGrade(int studentId, int courseId, int mids, int final, string semester = "2026 Payız")
        {
            if (mids < 0 || mids > 100 || final < 0 || final > 100)
            {
                TempData["Error"] = "Ballar 0 ilə 100 arasında olmalıdır!";
                return RedirectToAction(nameof(StudentsReportCard), new { courseId = courseId });
            }

            int total = (int)Math.Round((mids + final) / 2.0);
            string letterGrade = CalculateCalculateLetterGrade(total);

            var existingGrade = await _context.grades
                .FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseId == courseId);

            if (existingGrade != null)
            {
                existingGrade.Mids = mids;
                existingGrade.Final = final;
                existingGrade.Total = total;
                existingGrade.LetterGrade = letterGrade;
                existingGrade.Semester = semester;

                _context.Update(existingGrade);
                TempData["Success"] = "Ballar uğurla yeniləndi.";
            }
            else
            {
                var newGrade = new Grade
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    Mids = mids,
                    Final = final,
                    Total = total,
                    LetterGrade = letterGrade,
                    Semester = semester
                };

                await _context.AddAsync(newGrade);
                TempData["Success"] = "Yeni ballar uğurla daxil edildi.";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StudentsReportCard), new { courseId = courseId });
        }

        private string CalculateCalculateLetterGrade(int total)
        {
            if (total >= 91) return "A";
            if (total >= 81) return "B";
            if (total >= 71) return "C";
            if (total >= 61) return "D";
            if (total >= 51) return "E";
            return "F";
        }
    }
}