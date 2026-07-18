using System.Collections.Generic;
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
            var groupsList = await _context.Groups
                .Include(g => g.Students)
                .Include(g => g.Speciality)
                .Where(g => !g.isDeleted) // Silinməmiş qrupları gətiririk
                .ToListAsync();

            return View(groupsList);
        }

        // ==========================================
        // 2. YENİ QRUP YARATMA SƏHİFƏSİ (GET)
        // ==========================================
        public IActionResult Create()
        {
            ViewBag.Specialities = _context.specialities.Where(s => !s.isDeleted).ToList();
            return View();
        }

        // ==========================================
        // 3. YENİ QRUP YARATMA (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Group group)
        {
            // .NET-ə formdan gəlməyən əlaqəli obyektləri yoxlamamağı əmr edirik
            ModelState.Remove("Speciality");
            ModelState.Remove("Students");
            ModelState.Remove("Schedules");

            // İxtisasın seçilib-seçilmədiyini əl ilə yoxlayırıq
            if (group.SpecialityId == null || group.SpecialityId == 0)
            {
                ModelState.AddModelError("SpecialityId", "Zəhmət olmasa, qrupun ixtisasını seçin!");
            }

            if (ModelState.IsValid)
            {
                // Eyni adda qrupun təkrar yaradılmasının qarşısını alırıq
                var exists = await _context.Groups.AnyAsync(g => g.GroupName == group.GroupName && !g.isDeleted);
                if (exists)
                {
                    ModelState.AddModelError("GroupName", "Bu adda qrup artıq mövcuddur!");
                    ViewBag.Specialities = _context.specialities.Where(s => !s.isDeleted).ToList();
                    return View(group);
                }

                await _context.Groups.AddAsync(group);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Qrup uğurla yaradıldı!";
                return RedirectToAction(nameof(Index));
            }

            // Validasiya xətası varsa dropdown siyahısı boş qalmasın
            ViewBag.Specialities = _context.specialities.Where(s => !s.isDeleted).ToList();
            return View(group);
        }

        // ==========================================
        // 4. QRUPU SİLMƏK (POST)
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

            // Kritik Yoxlanış: Əgər bu qrupda hələ də tələbələr varsa, silməyə icazə vermirik
            var hasStudents = await _context.students.AnyAsync(s => s.GroupId == id && !s.isDeleted);
            if (hasStudents)
            {
                TempData["Error"] = "Bu qrupda aktiv tələbələr var! Qrupu silmək üçün əvvəlcə tələbələri başqa qrupa köçürməli və ya silməlisiniz.";
                return RedirectToAction(nameof(Index));
            }

            // Layihənin ümumi məntiqinə uyğun olaraq Soft-Delete və ya Hard-Delete edə bilərsən. 
            // Burada birbaşa bazadan silirik:
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Qrup uğurla silindi!";
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 🎯 5. QRUPUN İXTİSASINA UYGUN KURSLARI LİSTƏLƏYƏN METOD (GET)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ChooseSubjectsForGroup(int? id)
        {
            if (id == null) return BadRequest();

            var group = await _context.Groups
                .Include(g => g.Speciality)
                .Where(g => !g.isDeleted)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null) return NotFound();

            // Qrupun aid olduğu ixtisasa aid aktiv dərsləri gətiririk
            var filteredCourses = await _context.courses
                .Where(c => c.SpecialityId == group.SpecialityId && !c.isDeleted)
                .ToListAsync();

            // Checkbox-ların əvvəlcədən doldurulması məntiqi: 
            // Qrupdakı nümunə bir tələbənin seçdiyi fənləri götürürük
            var sampleStudent = await _context.students.FirstOrDefaultAsync(s => s.GroupId == id && !s.isDeleted);
            List<int> alreadySelectedCourseIds = new List<int>();

            if (sampleStudent != null)
            {
                alreadySelectedCourseIds = await _context.StudentSubjects
                    .Where(ss => ss.StudentId == sampleStudent.Id)
                    .Select(ss => ss.CourseId)
                    .ToListAsync();
            }

            ViewBag.Group = group;
            ViewBag.AlreadySelected = alreadySelectedCourseIds;

            return View(filteredCourses);
        }

        // ==========================================
        // 🎯 6. SEÇİLƏN KURSLARI QRUPDAKİ BÜTÜN TƏLƏBƏLƏRƏ TOPLU YAZAN METOD (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChooseSubjectsForGroup(int groupId, List<int> selectedCourseIds)
        {
            var group = await _context.Groups.Where(g => !g.isDeleted).FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null) return NotFound();

            // Qrupdakı bütün aktiv tələbələrin ID siyahısını çıxarırıq
            var studentIds = await _context.students
                .Where(s => s.GroupId == groupId && !s.isDeleted)
                .Select(s => s.Id)
                .ToListAsync();

            if (studentIds.Any())
            {
                // Qrup tələbələrinin köhnə fənn seçimlərini sıfırlayırıq
                var oldSelections = _context.StudentSubjects.Where(ss => studentIds.Contains(ss.StudentId));
                _context.StudentSubjects.RemoveRange(oldSelections);

                // Yeni dərsləri qrupdakı hər bir tələbəyə tək-tək mənsub edirik
                if (selectedCourseIds != null && selectedCourseIds.Any())
                {
                    foreach (var studentId in studentIds)
                    {
                        foreach (var courseId in selectedCourseIds)
                        {
                            var studentSubject = new StudentSubject
                            {
                                StudentId = studentId,
                                CourseId = courseId
                            };
                            await _context.StudentSubjects.AddAsync(studentSubject);
                        }
                    }
                }
                await _context.SaveChangesAsync();
                TempData["Success"] = "Qrupun fənləri uğurla yeniləndi!";
            }
            else
            {
                TempData["Error"] = "Bu qrupda fənn təyin ediləcək tələbə tapılmadı!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}