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

        // 1. Siyahıda yalnız kafedranı göstəririk (Courses birbaşa əlaqəsi silindi 🎯)
        public IActionResult ShowTeachersTable()
        {
            List<Teacher> teachers = _context.teachers
                .Include(t => t.Department)
                .Where(t => !t.isDeleted)
                .ToList();
            return View(teachers);
        }

        // 2. Create GET - Seçim qutusu üçün yalnız kafedraları doldururuq (Fənlər siyahısı ləğv edildi)
        public IActionResult Create()
        {
            ViewBag.Departments = _context.departments.Where(d => !d.isDeleted).ToList();
            return View();
        }

        // 3. Create POST - Müəllimi və Rolunu qeyd edirik
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTeacherVM vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = _context.departments.Where(d => !d.isDeleted).ToList();
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
                    Salary = vm.Salary,
                    Password = vm.Password,
                    DepartmentId = vm.DepartmentId,
                    MemberId = newIdentityUser.Id
                };

                _context.teachers.Add(newTeacher);
                await _context.SaveChangesAsync();

                return RedirectToAction("ShowTeachersTable", "Teacher");
            }

            ViewBag.Departments = _context.departments.Where(d => !d.isDeleted).ToList();
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

        // 4. Update GET - Mövcud məlumatları gətiririk
        public IActionResult Update(int? id)
        {
            if (id == null) return BadRequest();

            Teacher existTeacher = _context.teachers
                .Where(t => !t.isDeleted)
                .FirstOrDefault(c => c.Id == id);

            if (existTeacher == null) return NotFound();

            ViewBag.Departments = _context.departments.Where(d => !d.isDeleted).ToList();

            UpdateTeacherVM vm = new UpdateTeacherVM
            {
                Id = existTeacher.Id,
                FullName = existTeacher.FullName,
                Email = existTeacher.Email,
                Speciality = existTeacher.Speciality,
                Salary = existTeacher.Salary,
                DepartmentId = existTeacher.DepartmentId
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
                return View(vm);
            }

            Teacher existTeacher = _context.teachers
                .Where(t => !t.isDeleted)
                .FirstOrDefault(c => c.Id == id);

            if (existTeacher == null) return NotFound();

            existTeacher.FullName = vm.FullName;
            existTeacher.Email = vm.Email;
            existTeacher.Speciality = vm.Speciality;
            existTeacher.Salary = vm.Salary;
            existTeacher.DepartmentId = vm.DepartmentId;

            await _context.SaveChangesAsync();
            return RedirectToAction("ShowTeachersTable", "Teacher");
        }
    }
}