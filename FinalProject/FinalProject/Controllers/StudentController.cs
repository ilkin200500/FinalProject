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
            if (currentMember == null) return Unauthorized();

            var student = await _context.students
                .Include(s => s.Group)
                .FirstOrDefaultAsync(s => s.MemberId == currentMember.Id);

            if (student == null)
            {
                return NotFound("Tələbə profili tapılmadı.");
            }

            if (student.GroupId == null || student.Group == null)
            {
                TempData["Error"] = "Profilinizdə qrup təyinatı yoxdur. Lütfən administrasiyaya müraciət edin.";
                return View(new List<Grade>());
            }

            var grades = await _context.grades
                .Include(g => g.Course)
                .Where(g => g.StudentId == student.Id)
                .ToListAsync();

            ViewBag.StudentName = student.FullName;
            ViewBag.GroupName = student.Group.GroupName;
            return View(grades);
        }

        // ==========================================
        // 🔥 2. SEÇƏ BİLƏCƏYİ FƏNLƏR (İXTİSASA GÖRƏ)
        // ==========================================
        public async Task<IActionResult> AvailableCourses()
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var student = await _context.students
                .Include(s => s.Group)
                .FirstOrDefaultAsync(s => s.MemberId == currentMember.Id);

            if (student == null) return NotFound("Tələbə profili tapılmadı.");

            if (student.GroupId == null || student.Group == null)
            {
                TempData["Error"] = "Sizə hələ heç bir qrup və ixtisas təyin edilməyib!";
                return RedirectToAction(nameof(Index));
            }

            // Tələbənin artıq götürdüyü fənlərin ID siyahısı
            var enrolledCourseIds = await _context.courseRegistrations
                .Where(cr => cr.StudentId == student.Id)
                .Select(cr => cr.CourseId)
                .ToListAsync();

            // Möhtəşəm İxtisas Zirehi: Birbaşa ixtisasa aid aktiv fənləri çəkirik
            var availableCourses = await _context.courses
                .Where(c => c.SpecialityId == student.Group.SpecialityId
                         && !enrolledCourseIds.Contains(c.Id))
                .ToListAsync();

            ViewBag.StudentName = student.FullName;

            return View(availableCourses);
        }

        // ==========================================
        // 🔥 3. FƏNNİ GÖTÜR POST METODU
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollCourse(int courseId)
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var student = await _context.students
                .Include(s => s.Group)
                .FirstOrDefaultAsync(s => s.MemberId == currentMember.Id);

            if (student == null) return NotFound();
            if (student.GroupId == null || student.Group == null) return BadRequest();

            // ARXA FON TƏHLÜKƏSİZLİK ZİREHİ: Seçilən fənn tələbənin ixtisasına aid mi?
            var targetCourse = await _context.courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (targetCourse == null || targetCourse.SpecialityId != student.Group.SpecialityId)
            {
                TempData["Error"] = "Təhlükəsizlik Xətası: Yalnız öz ixtisasınıza aid fənləri seçə bilərsiniz!";
                return RedirectToAction(nameof(AvailableCourses));
            }

            var alreadyEnrolled = await _context.courseRegistrations
                .AnyAsync(cr => cr.StudentId == student.Id && cr.CourseId == courseId);

            if (alreadyEnrolled)
            {
                TempData["Error"] = "Siz bu fənnə artıq qeydiyyatdan keçmisiniz!";
                return RedirectToAction(nameof(AvailableCourses));
            }

            // 1. Addım: Cari qrup üçün schedules cədvəlində müəllim varmı deyə baxırıq
            var schedule = await _context.schedules
                .FirstOrDefaultAsync(s => s.CourseId == courseId && s.GroupId == student.GroupId);

            int assignedTeacherId;

            if (schedule != null)
            {
                assignedTeacherId = schedule.TeacherId;
            }
            else
            {
                // 2. Addım: Əgər bu qrupa hələ dərs salınmayıbsa, bu fənni tədris edən HƏR HANSI başqa bir müəllimi tapırıq
                var anyTeacherForCourse = await _context.schedules
                    .Where(s => s.CourseId == courseId)
                    .Select(s => s.TeacherId)
                    .FirstOrDefaultAsync();

                if (anyTeacherForCourse != 0)
                {
                    assignedTeacherId = anyTeacherForCourse;
                }
                else
                {
                    // 3. Addım: Əgər heç bir cədvəldə bu fənn yoxdursa, bazadakı ən birinci müəllimin ID-sini götürürük
                    var backupTeacherId = await _context.teachers.Select(t => t.Id).FirstOrDefaultAsync();

                    if (backupTeacherId == 0)
                    {
                        TempData["Error"] = "Sistemdə aktiv müəllim tapılmadı! Öncə müəllim əlavə edilməlidir.";
                        return RedirectToAction(nameof(AvailableCourses));
                    }

                    assignedTeacherId = backupTeacherId;
                }
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
                TeacherId = assignedTeacherId,
                Semester = "2026 Yaz",
                CreatedAt = DateTime.Now
            };

            await _context.courseRegistrations.AddAsync(registration);
            await _context.grades.AddAsync(grade);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"\"{targetCourse.CourseName}\" fənninə uğurla qeydiyyatdan keçdiniz!";
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
            if (student.GroupId == null)
            {
                TempData["Error"] = "Qrupunuz təyin edilmədiyi üçün dərs cədvəli mövcud deyil.";
                return RedirectToAction(nameof(Index));
            }

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
        // 🔔 6. TƏLƏBƏNİN BÜTÜN BİLDİRİŞLƏR SƏHİFƏSİ
        // =======================================================
        public async Task<IActionResult> Notifications()
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var student = await _context.students.FirstOrDefaultAsync(s => s.MemberId == currentMember.Id);
            if (student == null) return NotFound("Tələbə profili tapılmadı.");

            var notificationsList = await _context.notifications
                .Where(n => n.StudentId == student.Id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var unreadNotifications = notificationsList.Where(n => !n.IsRead).ToList();
            if (unreadNotifications.Any())
            {
                foreach (var item in unreadNotifications)
                {
                    item.IsRead = true;
                }

                await _context.SaveChangesAsync();
            }

            ViewBag.StudentName = student.FullName;
            return View(notificationsList);
        }

        // =======================================================
        // 📚 7. TƏLƏBƏNİN EV TAPŞIRIQLARI SƏHİFƏSİ (ASSIGNMENTS)
        // =======================================================
        public async Task<IActionResult> Assignments()
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var student = await _context.students
                .Include(s => s.Group)
                .FirstOrDefaultAsync(s => s.MemberId == currentMember.Id);

            if (student == null) return NotFound("Tələbə profili tapılmadı.");

            if (student.GroupId == null)
            {
                TempData["Error"] = "Qrupunuz təyin edilmədiyi üçün ev tapşırıqları görünmür.";
                return RedirectToAction(nameof(Index));
            }

            // Tələbənin öz qrupuna verilmiş bütün tapşırıqları çəkirik
            var studentAssignments = await _context.assignments
                .Include(a => a.Course)
                .Include(a => a.Teacher)
                .Where(a => a.GroupId == student.GroupId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewBag.StudentName = student.FullName;
            ViewBag.GroupName = student.Group?.GroupName;

            return View(studentAssignments);
        }
    }
}