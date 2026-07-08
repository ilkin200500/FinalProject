using FinalProject.Areas.AdminPanel.ViewModels;
using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    public class StudentController : Controller
    {
        // readonly etmək təhlükəsizlik üçün yaxşı təcrübədir (best practice)
        private readonly CourseDbContext _context;

        public StudentController(CourseDbContext context)
        {
            _context = context;
        }

        // Giriş səhifəsi - Tələbələrin Siyahısı
        public IActionResult ShowStudentsTable()
        {
            // Cədvəl adının (students) böyük-kiçik hərfinə bazada necədirsə elə diqqət et
            List<Student> students = _context.students.Where(t => !t.isDeleted).ToList();

            return View(students);
        }

        // Tələbə yaratmaq üçün səhifəni açan GET metodu
        public IActionResult Create()
        {
            return View();
        }

        // Formdan gələn məlumatları qəbul edən POST metodu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudentVM vm)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Daxil etdiyiniz məlumatda səhv var.");
                return View(vm);
            }

            Student newStudent = new Student
            {
                FullName = vm.FullName,
                Email = vm.Email,
                Speciality = vm.Speciality,
                Average = vm.Average,
                
            };

            // Düzəliş: Müvafiq DbSet-ə (students cədvəlinə) sinxron Add edildi
            _context.students.Add(newStudent);

            // Asinxron olaraq bazada dəyişikliklər qeyd edilir
            await _context.SaveChangesAsync();

            // Düzəliş: Parametrlərin sırası düzəldildi -> ("Action", "Controller")
            return RedirectToAction("ShowStudentsTable", "Student");
        }

        public  async Task<IActionResult> Delete(int? id)
        {
            Student existStudent = _context.students.Where(t => !t.isDeleted).FirstOrDefault(c=>c.Id==id);
            existStudent.isDeleted = true;
            await _context.SaveChangesAsync();
            return RedirectToAction("ShowStudentsTable", "Student");
        }


        public IActionResult Update(int?id)
        {
            Student existStudent = _context.students.Where(t => !t.isDeleted).FirstOrDefault(c => c.Id == id);
            UpdateStudentVM vm = new UpdateStudentVM
            {
                FullName = existStudent.FullName,
                Email = existStudent.Email,
                Speciality = existStudent.Speciality,
                Average = existStudent.Average,
            };
            return View(vm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Update(int?id,UpdateStudentVM vm)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "daxil etdiyiniz melumat yalnisdir");
                return View(vm);
            }
            Student existStudent = _context.students.Where(t => !t.isDeleted).FirstOrDefault(c => c.Id == id);
            existStudent.FullName=vm.FullName;
            existStudent.Speciality=vm.Speciality;
            existStudent.Email=vm.Email;
            existStudent.Average=vm.Average;
            await _context.SaveChangesAsync();
            return RedirectToAction("ShowStudentsTable", "Student");

        }
    }
}
