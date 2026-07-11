using FinalProject.Areas.AdminPanel.ViewModels;
using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    public class TeacherController : Controller
    {
        private CourseDbContext _context { get; }
        private readonly UserManager<Member> _userManager;

        public TeacherController(CourseDbContext context, UserManager<Member> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        // 1. Siyahıda həm kafedranı, həm də dərsləri göstərmək üçün Include edirik
        public IActionResult ShowTeachersTable()
        {
            List<Teacher> teacher = _context.teachers
                .Include(t => t.Department)
                .Include(t => t.Courses)
                .Where(t => !t.isDeleted)
                .ToList();
            return View(teacher);
        }

        // 2. Create GET - Seçim qutuları üçün ViewBag-ləri doldururuq
        public IActionResult Create()
        {
            ViewBag.Departments = _context.departments.Where(d => !d.isDeleted).ToList();
            ViewBag.Courses = _context.courses.Where(c => !c.isDeleted).ToList();
            return View();
        }

        // 3. Create POST - Müəllimi, Rolunu və Fənlərini qeyd edirik
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTeacherVM vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = _context.departments.Where(d => !d.isDeleted).ToList();
                ViewBag.Courses = _context.courses.Where(c => !c.isDeleted).ToList();
                return View(vm);
            }

            Member newIdentityUser = new Member
            {
                FullName = vm.FullName,
                UserName = vm.Email,
                Email = vm.Email,
                NormalizedEmail = vm.Email.ToUpper().Trim(),
                NormalizedUserName = vm.Email.ToUpper().Trim(),
                isActivated = true,
                EmailConfirmed = true
            };

            var identityResult = await _userManager.CreateAsync(newIdentityUser, vm.Password);

            if (identityResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(newIdentityUser, "Teacher");

                Teacher newTeacher = new Teacher
                {
                    FullName = vm.FullName,
                    Email = vm.Email,
                    Speciality = vm.Speciality,
                    // 🎯 LINE 84 XƏTASININ HƏLLİ: vm.Salary int olduğu üçün bura (decimal) əlavə etdik
                    Salary = vm.Salary,
                    Password = vm.Password,
                    DepartmentId = vm.DepartmentId,
                    MemberId = newIdentityUser.Id
                };

                // Çoxlu seçilmiş fənləri müəllimə bağlayırıq
                if (vm.CourseIds != null && vm.CourseIds.Any())
                {
                    var selectedCourses = _context.courses.Where(c => vm.CourseIds.Contains(c.Id)).ToList();
                    newTeacher.Courses = selectedCourses;
                }

                _context.teachers.Add(newTeacher);
                await _context.SaveChangesAsync();

                return RedirectToAction("ShowTeachersTable", "Teacher");
            }

            ViewBag.Departments = _context.departments.Where(d => !d.isDeleted).ToList();
            ViewBag.Courses = _context.courses.Where(c => !c.isDeleted).ToList();
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(vm);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();
            Teacher existTeacher = _context.teachers.Where(t => !t.isDeleted).FirstOrDefault(c => c.Id == id);
            if (existTeacher == null) return NotFound();

            existTeacher.isDeleted = true;
            await _context.SaveChangesAsync();
            return RedirectToAction("ShowTeachersTable", "Teacher");
        }

        // 4. Update GET - Mövcud məlumatları və seçilmiş dərsləri gətiririk
        public IActionResult Update(int? id)
        {
            if (id == null) return BadRequest();

            Teacher existTeacher = _context.teachers
                .Include(t => t.Courses)
                .Where(t => !t.isDeleted)
                .FirstOrDefault(c => c.Id == id);

            if (existTeacher == null) return NotFound();

            ViewBag.Departments = _context.departments.Where(d => !d.isDeleted).ToList();
            ViewBag.Courses = _context.courses.Where(c => !c.isDeleted).ToList();

            UpdateTeacherVM vm = new UpdateTeacherVM
            {
                Id = existTeacher.Id,
                FullName = existTeacher.FullName,
                Email = existTeacher.Email,
                Speciality = existTeacher.Speciality,
                Salary = (int)existTeacher.Salary, // Əgər VM daxilində int-dirsə, bura cast edirik
                DepartmentId = existTeacher.DepartmentId,
                CourseIds = existTeacher.Courses.Select(c => c.Id).ToList()
            };
            return View(vm);
        }

        // 5. Update POST - Dəyişiklikləri bazada yeniləyirik
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateTeacherVM vm, int? id)
        {
            if (id == null) return BadRequest();
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = _context.departments.Where(d => !d.isDeleted).ToList();
                ViewBag.Courses = _context.courses.Where(c => !c.isDeleted).ToList();
                return View(vm);
            }

            Teacher existTeacher = _context.teachers
                .Include(t => t.Courses)
                .Where(t => !t.isDeleted)
                .FirstOrDefault(c => c.Id == id);

            if (existTeacher == null) return NotFound();

            existTeacher.FullName = vm.FullName;
            existTeacher.Email = vm.Email;
            existTeacher.Speciality = vm.Speciality;

            // 🎯 LINE 177 XƏTASININ HƏLLİ: Burada da cast bərabərləşdirildi
            existTeacher.Salary = vm.Salary;

            existTeacher.DepartmentId = vm.DepartmentId;

            // Köhnə dərsləri təmizləyib, yeni seçilənləri bağlayırıq
            existTeacher.Courses.Clear();
            if (vm.CourseIds != null && vm.CourseIds.Any())
            {
                var selectedCourses = _context.courses.Where(c => vm.CourseIds.Contains(c.Id)).ToList();
                existTeacher.Courses = selectedCourses;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ShowTeachersTable", "Teacher");
        }
    }
}