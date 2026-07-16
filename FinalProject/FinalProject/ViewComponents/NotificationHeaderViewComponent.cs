using System.Linq;
using System.Threading.Tasks;
using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.ViewComponents
{
    public class NotificationHeaderViewComponent : ViewComponent
    {
        private readonly CourseDbContext _context;
        private readonly UserManager<Member> _userManager;

        public NotificationHeaderViewComponent(CourseDbContext context, UserManager<Member> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Giriş etmiş istifadəçini tapırıq
            var currentMember = await _userManager.GetUserAsync(HttpContext.User);
            if (currentMember == null)
            {
                return View(new List<Notification>());
            }

            // İstifadəçiyə bağlı tələbə profilini tapırıq
            var student = await _context.students
                .FirstOrDefaultAsync(s => s.MemberId == currentMember.Id);

            if (student == null)
            {
                return View(new List<Notification>());
            }

            // Son 5 oxunmamış və ya ümumi bildirişi çəkirik (zəng ikonunun altında sürətli baxış üçün)
            var notifications = await _context.notifications
                .Where(n => n.StudentId == student.Id)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Oxunmamış bildirişlərin ümumi sayını ViewBag-ə atırıq (zəngin üzərində qırmızı rəqəm yazmaq üçün)
            ViewBag.UnreadCount = await _context.notifications
                .CountAsync(n => n.StudentId == student.Id && !n.IsRead);

            return View(notifications);
        }
    }
}