using System.Linq;
using System.Threading.Tasks;
using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    [Authorize(Roles = "Admin")]
    public class GroupController : Controller
    {
        private readonly CourseDbContext _context;

        public GroupController(CourseDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. QRUPLARIN SİYAHISI
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var groupsList = await _context.Groups.ToListAsync();
            return View(groupsList);
        }

        // ==========================================
        // 2. YENİ QRUP YARATMA SƏHİFƏSİ (GET)
        // ==========================================
        public IActionResult Create()
        {
            return View();
        }

        // ==========================================
        // 3. YENİ QRUP YARATMA (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Group group)
        {
            if (ModelState.IsValid)
            {
                // Eyni adda qrupun təkrar yaradılmasının qarşısını alırıq
                var exists = await _context.Groups.AnyAsync(g => g.GroupName == group.GroupName);
                if (exists)
                {
                    ModelState.AddModelError("GroupName", "Bu adda qrup artıq mövcuddur!");
                    return View(group);
                }

                await _context.Groups.AddAsync(group);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Qrup uğurla yaradıldı!";
                return RedirectToAction(nameof(Index));
            }
            return View(group);
        }

        // ==========================================
        // 4. QRUPU SİLMƏK (POST) - YENİ ƏLAVƏ EDİLDİ 🎯
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                TempData["Error"] = "Silinmək istənən qrup tapılmadı!";
                return RedirectToAction(nameof(Index));
            }

            // ⚠️ Kritik Yoxlanış: Əgər bu qrupda hələ də tələbələr varsa, silməyə icazə vermirik
            var hasStudents = await _context.students.AnyAsync(s => s.GroupId == id);
            if (hasStudents)
            {
                TempData["Error"] = "Bu qrupda aktiv tələbələr var! Qrupu silmək üçün əvvəlcə tələbələri başqa qrupa köçürməli və ya silməlisiniz.";
                return RedirectToAction(nameof(Index));
            }

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Qrup uğurla silindi!";
            return RedirectToAction(nameof(Index));
        }
    }
}