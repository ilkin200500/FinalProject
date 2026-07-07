using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Areas.AdminPanel.Controllers
{

    //[Authorize(Roles ="Admin")]
    [Area("AdminPanel")]
    // [Route("Admin")] <-- Əgər belə bir sətir varsa, bunu mütləq sil və ya şərhə al!
    public class DashboardController : Controller
    {
        // Bura açıq şəkildə View-nun tam yolunu yazaraq sistemi məcbur edirik:
        public IActionResult Index()
        {
            return View();
        }
    }
}
