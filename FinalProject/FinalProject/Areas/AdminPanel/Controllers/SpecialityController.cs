using System.Linq;
using System.Threading.Tasks;
using FinalProject.Areas.AdminPanel.ViewModels;
using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    public class SpecialityController : Controller
    {
        private readonly CourseDbContext _context;

        public SpecialityController(CourseDbContext context)
        {
            _context = context;
        }

        // İxtisasların siyahısı (Yalnız aktiv olanlar)
        public IActionResult Index()
        {
            var model = _context.specialities
                .Where(s => !s.isDeleted)
                .ToList();

            return View(model);
        }

        // Yaratmaq üçün GET Metodu
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Formdan gələn datanı qəbul edən POST Metodu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSpecialityVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            Speciality newSpeciality = new Speciality
            {
                Name = vm.Name,
                isDeleted = false
            };

            _context.specialities.Add(newSpeciality);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 🎯 ƏLAVƏ OLUNDU: İxtisası silən (Soft Delete) metod
        public async Task<IActionResult> Delete(int id)
        {
            var speciality = await _context.specialities.FindAsync(id);

            if (speciality == null)
            {
                return NotFound();
            }

            // Tam silmək əvəzinə statusunu dəyişirik ki, Index-də görünməsin
            speciality.isDeleted = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}