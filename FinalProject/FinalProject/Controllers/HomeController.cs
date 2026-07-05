using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Controllers
{
    public class HomeController : Controller
    {
        private CourseDbContext _context { get; }
        public HomeController(CourseDbContext context)
        {
            _context= context;
        }
        public IActionResult Index()
        {
            List<Member>members=_context.members.Where(t=>!t.isDeleted).ToList();
            return View(members);
        }
    }
}
