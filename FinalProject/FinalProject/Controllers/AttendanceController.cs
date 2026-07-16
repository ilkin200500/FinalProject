using FinalProject.DAL;
using FinalProject.Models;
using FinalProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FinalProject.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly CourseDbContext _context;
        private readonly UserManager<Member> _userManager;

        public AttendanceController(CourseDbContext context, UserManager<Member> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // 👨‍🏫 MÜƏLLİM ÜÇÜN: DAVAMİYYƏT YAZMA SƏHİFƏSİ (GET)
        // ==========================================
        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public async Task<IActionResult> MarkAttendance(int? courseId)
        {
            if (courseId == null || courseId == 0)
            {
                TempData["Error"] = "Xəta: Kurs ID-si ötürülmədi.";
                return RedirectToAction("Index", "Teacher");
            }

            var studentsInCourse = await _context.courseRegistrations
                .Include(cr => cr.Student)
                .Where(cr => cr.CourseId == courseId.Value && cr.Student != null)
                .Select(cr => cr.Student)
                .ToListAsync();

            if (studentsInCourse == null || !studentsInCourse.Any())
            {
                TempData["Error"] = "Diqqət: Bu fənnə qeydiyyatdan keçmiş tələbə tapılmadı.";
                return RedirectToAction("Index", "Teacher");
            }

            var viewModel = new RollCallVM
            {
                CourseId = courseId.Value,
                Date = DateTime.Today,
                Students = studentsInCourse.Select(s => new StudentAttendanceSelection
                {
                    StudentId = s.Id,
                    StudentName = s.FullName,
                    IsPresent = true
                }).ToList()
            };

            return View(viewModel);
        }

        // ==========================================
        // 👨‍🏫 MÜƏLLİM ÜÇÜN: DAVAMİYYƏTİ YADDA SAXLA (POST)
        // ==========================================
        [Authorize(Roles = "Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(RollCallVM model)
        {
            if (model.Students == null || !model.Students.Any())
            {
                ModelState.AddModelError("", "Tələbə siyahısı boş ola bilməz.");
                return View(model);
            }

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
            TempData["Success"] = "Davamiyyət uğurla qeyd edildi.";

            return RedirectToAction("Index", "Teacher");
        }

        // ==========================================
        // 🎓 TƏLƏBƏ ÜÇÜN: DAVAMİYYƏT HESABATI SƏHİFƏSİ
        // ==========================================
        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> StudentReport()
        {
            var currentMember = await _userManager.GetUserAsync(User);
            if (currentMember == null) return Unauthorized();

            var student = await _context.students.FirstOrDefaultAsync(s => s.MemberId == currentMember.Id);
            if (student == null) return NotFound();

            // Tələbənin qeydiyyatdan keçdiyi fənlər
            var studentCourses = await _context.courseRegistrations
                .Include(cr => cr.Course)
                .Where(cr => cr.StudentId == student.Id && cr.Course != null)
                .Select(cr => cr.Course)
                .ToListAsync();

            // Tələbənin bütün davamiyyət qeydləri
            var totalAttendanceRecords = await _context.attendances
                .Where(a => a.StudentId == student.Id)
                .ToListAsync();

            // 🎯 MAKSİMAL İCAZƏ VERİLƏN QB LİMİTİ (9 limit)
            // Tələbənin qayıb sayı bu rəqəmdən çox olarsa, limit keçilmiş sayılır.
            int maxAllowedAbsents = 9;

            var reportList = studentCourses.Select(course =>
            {
                // Keçirilən ümumi dərs saatı
                int totalClasses = totalAttendanceRecords.Count(a => a.CourseId == course.Id);

                // Tələbənin aldığı qayıb (QB) sayı
                int absentCount = totalAttendanceRecords.Count(a => a.CourseId == course.Id && !a.IsPresent);

                // 🎯 LİMİT YOXLANILMASI: 
                // Əgər qayıb sayı (absentCount) 9-dan böyükdürsə IsFailed = true (kəsilir)
                bool isFailed = absentCount > maxAllowedAbsents;

                return new AttendanceReportVM
                {
                    CourseName = course.CourseName,
                    TotalClasses = totalClasses,
                    AbsentCount = absentCount,
                    Percentage = 0, // Faiz hesabatından istifadə etmədiyimiz üçün bura statik 0 ötürürük
                    IsFailed = isFailed
                };
            }).ToList();

            ViewBag.StudentName = student.FullName;
            return View(reportList);
        }
    }
}