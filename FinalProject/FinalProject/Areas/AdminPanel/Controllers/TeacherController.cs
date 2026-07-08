using FinalProject.Areas.AdminPanel.ViewModels;
using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    public class TeacherController : Controller
    {
        private CourseDbContext _context { get; }
        public TeacherController(CourseDbContext context)
        {
            _context= context;
            
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ShowTeachersTable()
        {
            List<Teacher> teacher = _context.teachers.Where(t => !t.isDeleted).ToList();

            return View(teacher);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Create(CreateTeacherVM vm)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "daxil etdiyiniz melumat yalnisdir");
                return View(vm);
            }
            Teacher newTeacher = new Teacher
            {
                FullName=vm.FullName,
                Email=vm.Email,
                Speciality=vm.Speciality,
                Salary=vm.Salary,
            };

            _context.Add(newTeacher);
            await _context.SaveChangesAsync();
            return RedirectToAction("ShowTeachersTable", "Teacher");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            Teacher existTeacher =_context.teachers.Where(t => !t.isDeleted).FirstOrDefault(c=>c.Id==id);
            existTeacher.isDeleted = true;
            await _context.SaveChangesAsync();
            return RedirectToAction("ShowTeachersTable", "Teacher");
        }

        public IActionResult Update(int?id)
        {
            Teacher existTeacher = _context.teachers.Where(t => !t.isDeleted).FirstOrDefault(c => c.Id == id);
            UpdateTeacherVM vm = new UpdateTeacherVM
            {
                FullName=existTeacher.FullName,
                Email=existTeacher.Email,
                Speciality = existTeacher.Speciality,
                Salary = existTeacher.Salary,
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Update(UpdateTeacherVM vm, int? id)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "daxil etdiyiniz melumat yalnisdir");
                return View(vm);
            }
            Teacher existTeacher = _context.teachers.Where(t => !t.isDeleted).FirstOrDefault(c => c.Id == id);
            existTeacher.FullName = vm.FullName;
            existTeacher.Email=vm.Email;
            existTeacher.Speciality = vm.Speciality;
            existTeacher.Salary = vm.Salary;

            await _context.SaveChangesAsync();
            return RedirectToAction("ShowTeachersTable", "Teacher");



        }
    }
}

