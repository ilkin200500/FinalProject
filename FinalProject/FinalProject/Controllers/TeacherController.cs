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
        // 1. MÜƏLLİMİN ÖZ FƏNLƏRİNİN VƏ QRUPLARININ SİYAHISI
        // =======================================================
        public async Task<IActionResult> Index()
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var teacher = await _context.teachers.FirstOrDefaultAsync(t => t.MemberId == currentMember.Id);
            if (teacher == null) return View("Error");

            // Müəllimin dərs keçdiyi cədvəl qeydlərini (Fənn və Qrup daxil) çəkirik
            var myClasses = await _context.schedules
                .Include(s => s.Course)
                .Include(s => s.Group)
                .Where(s => s.TeacherId == teacher.Id && s.Course != null && s.Group != null)
                .ToListAsync();

            // Eyni fənn və qrup kombinasiyalarını qruplaşdırıb tək siyahıya salırıq
            var uniqueClasses = myClasses
                .GroupBy(s => new { s.CourseId, s.GroupId })
                .Select(g => g.First())
                .ToList();

            ViewBag.TeacherName = teacher.FullName;
            return View(uniqueClasses);
        }

        // =======================================================
        // 2. SEÇİLMİŞ FƏNN VƏ QRUP ÜZRƏ TƏLƏBƏLƏRİN SİYAHISI (Jurnal)
        // =======================================================
        public async Task<IActionResult> StudentsReportCard(int courseId, int groupId)
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var teacher = await _context.teachers.FirstOrDefaultAsync(t => t.MemberId == currentMember.Id);
            if (teacher == null) return View("Error");

            // Təhlükəsizlik: Müəllim həqiqətən bu qrupa və fənnə dərs keçirmi?
            bool hasAccess = await _context.schedules
                .AnyAsync(s => s.TeacherId == teacher.Id && s.CourseId == courseId && s.GroupId == groupId);

            if (!hasAccess) return Forbid();

            var course = await _context.courses.FirstOrDefaultAsync(c => c.Id == courseId);
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            if (course == null || group == null) return NotFound();

            ViewBag.CourseId = courseId;
            ViewBag.GroupId = groupId;
            ViewBag.CourseName = course.CourseName;
            ViewBag.GroupName = group.GroupName;

            // Yalnız bu qrupda olan və bu fənni götürən tələbələri seçirik
            var registeredStudents = await _context.courseRegistrations
                .Include(cr => cr.Student)
                .Where(cr => cr.CourseId == courseId && cr.Student != null && cr.Student.GroupId == groupId)
                .Select(cr => cr.Student)
                .ToListAsync();

            if (registeredStudents == null || !registeredStudents.Any())
            {
                TempData["Error"] = $"Diqqət: '{group.GroupName}' qrupundan bu fənnə qeydiyyatdan keçmiş tələbə tapılmadı.";
                return RedirectToAction("Index");
            }

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

        // =======================================================
        // 3. QİYMƏT DAXİL ETMƏK VƏ YA YENİLƏMƏK (FORM POST)
        // =======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignGrade(int studentId, int courseId, int groupId, int mids, int? final, string semester = "2026 Yaz")
        {
            if (studentId <= 0 || courseId <= 0 || groupId <= 0)
            {
                TempData["Error"] = "Məlumatlar tam deyil və ya düzgün ötürülməyib!";
                return RedirectToAction("StudentsReportCard", new { courseId = courseId, groupId = groupId });
            }

            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var teacher = await _context.teachers.FirstOrDefaultAsync(t => t.MemberId == currentMember.Id);
            if (teacher == null) return View("Error");

            if (mids < 0 || mids > 50 || (final.HasValue && (final.Value < 0 || final.Value > 50)))
            {
                TempData["Error"] = "Giriş balı və Final balı 0 ilə 50 arasında olmalıdır!";
                return RedirectToAction("StudentsReportCard", new { courseId = courseId, groupId = groupId });
            }

            try
            {
                var course = await _context.courses.FirstOrDefaultAsync(c => c.Id == courseId);
                string courseName = course?.CourseName ?? "Fənn";

                var existingGrade = await _context.grades
                    .FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseId == courseId);

                bool isNewGrade = existingGrade == null;

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

                // 🔔 Tələbə üçün bildiriş paneli məntiqi
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
                catch (Exception) { /* Xəta gizlədilir */ }

                TempData["Success"] = "Ballar uğurla yadda saxlanıldı.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Baza xətası: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return RedirectToAction("StudentsReportCard", new { courseId = courseId, groupId = groupId });
        }

        // =======================================================
        // 4. MÜƏLLİMİN VERDİYİ EV TAPŞIRIQLARI (LIST)
        // =======================================================
        public async Task<IActionResult> Assignments()
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var teacher = await _context.teachers.FirstOrDefaultAsync(t => t.MemberId == currentMember.Id);
            if (teacher == null) return View("Error");

            var assignmentsList = await _context.assignments
                .Include(a => a.Course)
                .Include(a => a.Group)
                .Where(a => a.TeacherId == teacher.Id)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(assignmentsList);
        }

        // =======================================================
        // 5. YENİ EV TAPŞIRIĞI YARATMAQ (GET)
        // =======================================================
        public async Task<IActionResult> CreateAssignment()
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var teacher = await _context.teachers.FirstOrDefaultAsync(t => t.MemberId == currentMember.Id);
            if (teacher == null) return View("Error");

            var myClasses = await _context.schedules
                .Include(s => s.Course)
                .Include(s => s.Group)
                .Where(s => s.TeacherId == teacher.Id && s.Course != null && s.Group != null)
                .ToListAsync();

            ViewBag.Courses = myClasses.Select(s => s.Course).DistinctBy(c => c.Id).ToList();
            ViewBag.Groups = myClasses.Select(s => s.Group).DistinctBy(g => g.Id).ToList();

            return View();
        }

        // =======================================================
        // 6. YENİ EV TAPŞIRIĞI YARATMAQ (POST)
        // =======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssignment(Assignment assignment)
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var teacher = await _context.teachers.FirstOrDefaultAsync(t => t.MemberId == currentMember.Id);
            if (teacher == null) return View("Error");

            if (assignment.DueDate <= DateTime.Now)
            {
                ModelState.AddModelError("DueDate", "Bitmə tarixi keçmiş bir tarix ola bilməz!");
            }

            if (ModelState.IsValid)
            {
                assignment.TeacherId = teacher.Id;
                assignment.CreatedAt = DateTime.Now;

                await _context.assignments.AddAsync(assignment);
                await _context.SaveChangesAsync();

                // 🔔 Tapşırıq yaradılan kimi həmin qrupun tələbələrinə bildiriş daxil olur
                try
                {
                    var course = await _context.courses.FindAsync(assignment.CourseId);
                    var studentsInGroup = await _context.students
                        .Where(s => s.GroupId == assignment.GroupId)
                        .ToListAsync();

                    foreach (var student in studentsInGroup)
                    {
                        var notification = new Notification
                        {
                            StudentId = student.Id,
                            TeacherId = teacher.Id,
                            Title = "Yeni Ev Tapşırığı!",
                            Message = $"\"{course?.CourseName}\" fənnindən yeni tapşırıq: \"{assignment.Title}\". Son tarix: {assignment.DueDate:dd.MM.yyyy HH:mm}",
                            IsRead = false,
                            CreatedAt = DateTime.Now
                        };
                        await _context.notifications.AddAsync(notification);
                    }
                    await _context.SaveChangesAsync();
                }
                catch (Exception) { /* Xəta idarə olunur */ }

                TempData["Success"] = "Ev tapşırığı uğurla yaradıldı!";
                return RedirectToAction(nameof(Assignments));
            }

            var myClasses = await _context.schedules
                .Include(s => s.Course)
                .Include(s => s.Group)
                .Where(s => s.TeacherId == teacher.Id && s.Course != null && s.Group != null)
                .ToListAsync();

            ViewBag.Courses = myClasses.Select(s => s.Course).DistinctBy(c => c.Id).ToList();
            ViewBag.Groups = myClasses.Select(s => s.Group).DistinctBy(g => g.Id).ToList();

            return View(assignment);
        }
    }
}