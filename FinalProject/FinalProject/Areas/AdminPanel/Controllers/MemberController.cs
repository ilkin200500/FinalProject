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
        
       

       
    }
}
