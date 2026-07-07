using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace FinalProject.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    public class MemberController : Controller
    {
        private CourseDbContext _context { get; }
        public MemberController(CourseDbContext context)
        {
            _context = context;
            
        }
        public IActionResult ShowStudentsTable()
        {
            List<Student>students=_context.students.Where(t=>!t.isDeleted).ToList();
            
            return View(students);
        }

        public IActionResult ShowTeachersTable()
        {
            List<Teacher> teacher = _context.teachers.Where(t => !t.isDeleted).ToList();

            return View(teacher);
        }
    }
}
