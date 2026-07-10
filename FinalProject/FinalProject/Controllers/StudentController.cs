using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Controllers
{
    [Authorize(Roles ="Student")]
    public class StudentController : Controller
    {
        
        public IActionResult Index()
        {
            return View();
        }
    }
}
