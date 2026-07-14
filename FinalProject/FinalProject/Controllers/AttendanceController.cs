using FinalProject.DAL;
using FinalProject.Models;
using FinalProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly CourseDbContext _context;

        public AttendanceController(CourseDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 👨‍🏫 MÜƏLLİM REJİMİ ÜÇÜN FUNKSİYALAR
        // ==========================================

        // 1. Müəllim fənni seçəndə tələbələrin siyahısını çıxaran GET funksiyası
        [HttpGet]
        public async Task<IActionResult> MarkAttendance(int courseId)
        {
            // Fənnə qeydiyyatdan keçmiş tələbələri bazadan çəkirik
            var studentsInCourse = await _context.courseRegistrations
                .Where(cr => cr.CourseId == courseId)
                .Select(cr => cr.Student)
                .ToListAsync();

            if (studentsInCourse == null || !studentsInCourse.Any())
            {
                TempData["Error"] = "Bu fənnə qeydiyyatdan keçmiş tələbə tapılmadı.";
                return RedirectToAction("Index", "Home");
            }

            var viewModel = new RollCallVM
            {
                CourseId = courseId,
                Date = DateTime.Today,
                Students = studentsInCourse.Select(s => new StudentAttendanceSelection
                {
                    StudentId = s.Id,
                    StudentName = s.FullName, // Sənin modelindəki FullName sahəsi
                    IsPresent = true // Default olaraq hamını gəlib qeyd edirik
                }).ToList()
            };

            return View(viewModel);
        }

        // 2. Müəllim checkbox-ları işarələyib "Yadda Saxla" düyməsinə basanda işə düşən POST funksiyası
        [HttpPost]
        public async Task<IActionResult> MarkAttendance(RollCallVM model)
        {
            if (model.Students == null || !model.Students.Any())
            {
                ModelState.AddModelError("", "Tələbə siyahısı boş ola bilməz.");
                return View(model);
            }

            // Hər bir tələbə üçün dövr qurub bazanın 'attendances' cədvəlinə yazırıq
            foreach (var student in model.Students)
            {
                var attendance = new Attendance
                {
                    StudentId = student.StudentId,
                    CourseId = model.CourseId,
                    Date = model.Date,
                    IsPresent = student.IsPresent
                };

                _context.attendances.Add(attendance);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Davamiyyət uğurla yadda saxlanıldı.";

            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // 🎓 TƏLƏBƏ REJİMİ ÜÇÜN FUNKSİYA (TAM DÜZƏLDİLDİ 🎯)
        // ==========================================

        // 3. Tələbə öz profilinə girəndə bütün fənlər üzrə qayıb limitini və statistikasını görən GET funksiyası
        [HttpGet]
        public async Task<IActionResult> StudentReport(int studentId)
        {
            // Tələbənin qeydiyyatdan keçdiyi bütün fənləri tapırıq
            var studentCourses = await _context.courseRegistrations
                .Where(cr => cr.StudentId == studentId)
                .Select(cr => cr.Course)
                .ToListAsync();

            // Tələbənin indiyə qədər olan bütün davamiyyət qeydlərini çəkirik
            var totalAttendanceRecords = await _context.attendances
                .Where(a => a.StudentId == studentId)
                .ToListAsync();

            // "AttendanceReportVM" siyahısı hazırlayırıq
            var reportList = studentCourses.Select(course =>
            {
                // Bu fənnə aid olan ümumi dərslərin (qeydlərin) sayı
                int totalClasses = totalAttendanceRecords.Count(a => a.CourseId == course.Id);

                // Tələbənin bu fəndən aldığı qayıbların (IsPresent == false) sayısı
                int absentCount = totalAttendanceRecords.Count(a => a.CourseId == course.Id && !a.IsPresent);

                // Tələbənin dərsdə olduğu (true) günlərin sayısı
                int presentCount = totalClasses - absentCount;

                // Davamiyyət faizinin hesablanması (Sıfıra bölünmə xətasından qorunmaq üçün yoxlayırıq)
                double attendancePercentage = totalClasses > 0
                    ? ((double)presentCount / totalClasses) * 100
                    : 100;

                // Limit yoxlanışı (məsələn: faiz 75-dən aşağıdırsa kəsilir)
                bool isFailed = attendancePercentage < 75;

                return new AttendanceReportVM
                {
                    CourseName = course.CourseName,
                    TotalClasses = totalClasses,
                    AbsentCount = absentCount,
                    // 🎯 XƏTANIN HƏLLİ: Hesablanan double dəyəri yuvarlaqlaşdırıb int-ə explicit cast edirik
                    Percentage = (int)Math.Round(attendancePercentage),
                    IsFailed = isFailed
                };
            }).ToList();

            // Həm View-ya, həm də ViewBag-ə eyni tipli, düzgün obyekti göndəririk
            ViewBag.Report = reportList;

            return View(reportList);
        }
    }
}