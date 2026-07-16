using System.Linq;
using System.Threading.Tasks;
using FinalProject.Areas.AdminPanel.ViewModels;
using FinalProject.DAL;
using FinalProject.Models;
using FinalProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        // Yenilənmiş Siyahı Metodu - Axtarışla Birlikdə
        public IActionResult ShowStudentsTable(string search)
        {
            // 1. Silinməmiş tələbələri çəkmək üçün query qururuq
            var studentQuery = _context.students
                .Include(s => s.Group)
                .Where(t => !t.isDeleted);

            // 2. Əgər axtarış qutusuna nəsə yazılıbsa, FullName-ə görə filtrləyirik
            if (!string.IsNullOrEmpty(search))
            {
                studentQuery = studentQuery.Where(s => s.FullName.Contains(search));
            }

            // 3. ViewModel-i formalaşdırırıq
            StudentListVM vm = new StudentListVM
            {
                SearchText = search,
                Students = studentQuery.ToList() // Filtrlənmiş siyahını gətirir
            };

            // 4. ViewModel-i View-ya göndəririk
            return View(vm);
        }

        // Tələbə yaratmaq üçün GET metodu
        public IActionResult Create()
        {
            ViewBag.Groups = _context.Groups.Where(g => !g.isDeleted).ToList();
            return View();
        }

        // Formdan gələn məlumatları qəbul edən POST metodu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudentVM vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Groups = _context.Groups.Where(g => !g.isDeleted).ToList();
                return View(vm);
            }

            // 1. ADDIM: Identity cədvəli üçün Member obyektini qururuq
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

            // Şifrəni Identity metodu ilə bazaya yazırıq
            var identityResult = await _userManager.CreateAsync(newIdentityUser, "Student123!");

            if (identityResult.Succeeded)
            {
                // Tələbəyə sistemdə "Student" rolunu veririk
                await _userManager.AddToRoleAsync(newIdentityUser, "Student");

                // 2. ADDIM: Profil məlumatlarını dbo.students cədvəlinə yazırıq
                Student newStudent = new Student
                {
                    FullName = vm.FullName,
                    Email = vm.Email,
                    Speciality = vm.Speciality,
                    Average = vm.Average,
                    BirthDate = vm.BirthDate,
                    PhoneNumber = vm.PhoneNumber,
                    Gender = vm.Gender,
                    GroupId = vm.GroupId,
                    StudentCode = vm.StudentCode,
                    MemberId = newIdentityUser.Id
                };

                _context.students.Add(newStudent);
                await _context.SaveChangesAsync();

                return RedirectToAction("ShowStudentsTable", "Student");
            }

            // Əgər Identity tərəfdə nəsə xəta olarsa
            ViewBag.Groups = _context.Groups.Where(g => !g.isDeleted).ToList();
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

        // Tələbəni redaktə etmək üçün GET metodu
        public IActionResult Update(int? id)
        {
            if (id == null) return BadRequest();

            Student existStudent = _context.students.Where(t => !t.isDeleted).FirstOrDefault(c => c.Id == id);
            if (existStudent == null) return NotFound();

            ViewBag.Groups = _context.Groups.Where(g => !g.isDeleted).ToList();

            UpdateStudentVM vm = new UpdateStudentVM
            {
                FullName = existStudent.FullName,
                Email = existStudent.Email,
                Speciality = existStudent.Speciality,
                Average = existStudent.Average,
                PhoneNumber = existStudent.PhoneNumber,
                Gender = existStudent.Gender,
                GroupId = existStudent.GroupId
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
                ViewBag.Groups = _context.Groups.Where(g => !g.isDeleted).ToList();
                return View(vm);
            }

            Student existStudent = _context.students.Where(t => !t.isDeleted).FirstOrDefault(c => c.Id == id);
            if (existStudent == null) return NotFound();

            existStudent.FullName = vm.FullName;
            existStudent.Speciality = vm.Speciality;
            existStudent.Email = vm.Email;
            existStudent.Average = vm.Average;
            existStudent.PhoneNumber = vm.PhoneNumber;
            existStudent.Gender = vm.Gender;
            existStudent.GroupId = vm.GroupId;

            await _context.SaveChangesAsync();
            return RedirectToAction("ShowStudentsTable", "Student");
        }
    }
}