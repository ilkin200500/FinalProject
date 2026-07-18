using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.Areas.AdminPanel.ViewModels;
using FinalProject.DAL;
using FinalProject.Models;
using FinalProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    public class StudentController : Controller
    {
        private readonly CourseDbContext _context;
        private readonly UserManager<Member> _userManager;

        public StudentController(CourseDbContext context, UserManager<Member> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Siyahı Metodu
        public IActionResult ShowStudentsTable(string search)
        {
            var studentQuery = _context.students
                .Include(s => s.Group)
                .Include(s => s.Speciality)
                .Where(t => !t.isDeleted);

            if (!string.IsNullOrEmpty(search))
            {
                studentQuery = studentQuery.Where(s => s.FullName.Contains(search));
            }

            StudentListVM vm = new StudentListVM
            {
                SearchText = search,
                Students = studentQuery.ToList()
            };

            return View(vm);
        }

        // Tələbə yaratmaq üçün GET metodu
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Groups = _context.Groups
                .Where(g => !g.isDeleted)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.GroupName
                }).ToList();

            ViewBag.Specialities = _context.specialities
                .Where(s => !s.isDeleted)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToList();

            return View();
        }

        // Formdan gələn məlumatları qəbul edən POST metodu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudentVM vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Groups = _context.Groups
                    .Where(g => !g.isDeleted)
                    .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.GroupName }).ToList();

                ViewBag.Specialities = _context.specialities
                    .Where(s => !s.isDeleted)
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();

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
                EmailConfirmed = true,
            };

            var identityResult = await _userManager.CreateAsync(newIdentityUser, "Student123!");

            if (identityResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(newIdentityUser, "Student");

                Student newStudent = new Student
                {
                    FullName = vm.FullName,
                    Email = vm.Email,
                    Average = vm.Average,
                    BirthDate = vm.BirthDate,
                    PhoneNumber = vm.PhoneNumber,
                    Gender = vm.Gender,
                    GroupId = vm.GroupId,
                    StudentCode = vm.StudentCode,
                    MemberId = newIdentityUser.Id,
                    SpecialityId = vm.SpecialityId
                };

                _context.students.Add(newStudent);
                await _context.SaveChangesAsync();

                return RedirectToAction("ShowStudentsTable", "Student");
            }

            ViewBag.Groups = _context.Groups
                .Where(g => !g.isDeleted)
                .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.GroupName }).ToList();

            ViewBag.Specialities = _context.specialities
                .Where(s => !s.isDeleted)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();

            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(vm);
        }

        // Tələbəni silən (Soft Delete) metod
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            Student existStudent = _context.students.Where(t => !t.isDeleted).FirstOrDefault(c => c.Id == id);
            if (existStudent == null) return NotFound();

            existStudent.isDeleted = true;
            await _context.SaveChangesAsync();
            return RedirectToAction("ShowStudentsTable", "Student");
        }

        // Redaktə etmək üçün GET metodu
        [HttpGet]
        public IActionResult Update(int? id)
        {
            if (id == null) return BadRequest();

            Student existStudent = _context.students.Where(t => !t.isDeleted).FirstOrDefault(c => c.Id == id);
            if (existStudent == null) return NotFound();

            ViewBag.Groups = _context.Groups
                .Where(g => !g.isDeleted)
                .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.GroupName }).ToList();

            ViewBag.Specialities = _context.specialities
                .Where(s => !s.isDeleted)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();

            UpdateStudentVM vm = new UpdateStudentVM
            {
                Id = existStudent.Id,
                FullName = existStudent.FullName,
                Email = existStudent.Email,
                Average = existStudent.Average,
                PhoneNumber = existStudent.PhoneNumber,
                Gender = existStudent.Gender,
                GroupId = existStudent.GroupId,
                SpecialityId = existStudent.SpecialityId,
                BirthDate = existStudent.BirthDate,
                StudentCode = existStudent.StudentCode,
                MemberId = existStudent.MemberId
            };
            return View(vm);
        }

        // Redaktə olunmuş məlumatları qəbul edən POST metodu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, UpdateStudentVM vm)
        {
            if (id == null) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Groups = _context.Groups
                    .Where(g => !g.isDeleted)
                    .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.GroupName }).ToList();

                ViewBag.Specialities = _context.specialities
                    .Where(s => !s.isDeleted)
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();

                return View(vm);
            }

            Student existStudent = _context.students.Where(t => !t.isDeleted).FirstOrDefault(c => c.Id == id);
            if (existStudent == null) return NotFound();

            existStudent.FullName = vm.FullName;
            existStudent.Email = vm.Email;
            existStudent.Average = vm.Average;
            existStudent.PhoneNumber = vm.PhoneNumber;
            existStudent.Gender = vm.Gender;
            existStudent.GroupId = vm.GroupId;
            existStudent.SpecialityId = vm.SpecialityId;
            existStudent.BirthDate = vm.BirthDate;

            await _context.SaveChangesAsync();
            return RedirectToAction("ShowStudentsTable", "Student");
        }
    }
}