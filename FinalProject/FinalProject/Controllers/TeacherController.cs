using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
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

        // =======================================================
        // 1. MÜƏLLİMİN ÖZ FƏNLƏRİNİN SİYAHISI (Index.cshtml-i açır)
        // =======================================================
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

        // =======================================================
        // 2. SEÇİLMİŞ FƏNN ÜZRƏ TƏLƏBƏLƏRİN SİYAHISI (Qiymət Səhifəsi)
        // =======================================================
        public async Task<IActionResult> StudentsReportCard(int courseId)
        {
            var course = await _context.courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null) return NotFound();

            ViewBag.CourseId = courseId;
            ViewBag.CourseName = course.CourseName;

            var registeredStudents = await _context.courseRegistrations
                .Include(cr => cr.Student)
                .Where(cr => cr.CourseId == courseId && cr.Student != null)
                .Select(cr => cr.Student)
                .ToListAsync();

            if (registeredStudents == null || !registeredStudents.Any())
            {
                TempData["Error"] = "Diqqət: Bu fənnə qeydiyyatdan keçmiş tələbə tapılmadı.";
                return RedirectToAction("Index");
            }

            var currentGrades = await _context.grades
                .Where(g => g.CourseId == courseId)
                .ToListAsync();

            // Select daxilindən LetterGrade təyinini tamamilə qaldırdıq.
            // Model obyekti yaradılan kimi Total və LetterGrade-i özü hesablayacaq.
            var jurnals = registeredStudents.Select(student => {
                var grade = currentGrades.FirstOrDefault(g => g.StudentId == student.Id);
                return new Grade
                {
                    Id = grade?.Id ?? 0,
                    StudentId = student.Id,
                    Student = student,
                    CourseId = courseId,
                    Mids = grade?.Mids ?? 0,
                    Final = grade?.Final, // Əgər yoxdursa null gedir
                    Semester = grade?.Semester ?? "2026 Yaz"
                };
            }).ToList();

            return View(jurnals);
        }

        // ==========================================
        // 3. QİYMƏT DAXİL ETMƏK VƏ YA YENİLƏMƏK (FORM POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignGrade(int studentId, int courseId, int mids, int? final, string semester = "2026 Yaz")
        {
            if (studentId <= 0 || courseId <= 0)
            {
                TempData["Error"] = "Tələbə və ya Fənn məlumatı düzgün deyil!";
                return RedirectToAction("StudentsReportCard", new { courseId = courseId });
            }

            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var teacher = await _context.teachers.FirstOrDefaultAsync(t => t.MemberId == currentMember.Id);
            if (teacher == null) return View("Error");

            if (mids < 0 || mids > 50 || (final.HasValue && (final.Value < 0 || final.Value > 50)))
            {
                TempData["Error"] = "Giriş balı və Final balı 0 ilə 50 arasında olmalıdır!";
                return RedirectToAction("StudentsReportCard", new { courseId = courseId });
            }

            try
            {
                var course = await _context.courses.FirstOrDefaultAsync(c => c.Id == courseId);
                string courseName = course?.CourseName ?? "Fənn";

                var existingGrade = await _context.grades
                    .FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseId == courseId);

                bool isNewGrade = existingGrade == null;

                // Bildiriş üçün müvəqqəti obyekt yaradıb hərfi qiyməti modelin özündən öyrənirik
                var tempGradeForNotif = new Grade { Mids = mids, Final = final };
                string letterGrade = tempGradeForNotif.LetterGrade;
                int total = tempGradeForNotif.Total;

                if (!isNewGrade)
                {
                    existingGrade.Mids = mids;
                    existingGrade.Final = final;
                    existingGrade.Semester = semester;
                    existingGrade.TeacherId = teacher.Id;

                    _context.Update(existingGrade);
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
                }

                await _context.SaveChangesAsync();

                // 🔔 Bildiriş göndərmə məntiqi
                try
                {
                    string notifTitle = isNewGrade ? "Yeni Qiymət Daxil Edildi" : "Qiymətiniz Yeniləndi";
                    string finalScoreText = final.HasValue ? final.Value.ToString() : "Daxil edilməyib";
                    string notifMessage = isNewGrade
                        ? $"\"{courseName}\" fənnindən qiymətiniz daxil edildi. Giriş: {mids}, Final: {finalScoreText}. Ümumi: {total} ({letterGrade})."
                        : $"\"{courseName}\" fənnindən qiymətiniz yeniləndi. Yeni Giriş: {mids}, Final: {finalScoreText}. Ümumi: {total} ({letterGrade}).";

                    var notification = new Notification
                    {
                        StudentId = studentId,
                        TeacherId = teacher.Id,
                        Title = notifTitle,
                        Message = notifMessage,
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };

                    await _context.notifications.AddAsync(notification);
                    await _context.SaveChangesAsync();
                }
                catch (Exception) { /* Log xətası */ }

                TempData["Success"] = "Ballar uğurla yadda saxlanıldı.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Verilənlər bazası xətası: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return RedirectToAction("StudentsReportCard", new { courseId = courseId });
        }
    }
}