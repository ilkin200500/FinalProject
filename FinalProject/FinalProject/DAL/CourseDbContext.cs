using FinalProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.DAL
{
    // Bura mütləq <Member, IdentityRole<int>, int> yazılmalıdır ki, hər yerdə ID 'int' olsun
    public class CourseDbContext : IdentityDbContext<Member, IdentityRole<int>, int>
    {
        public CourseDbContext(DbContextOptions<CourseDbContext> options) : base(options)
        {
        }

        // 💡 QEYD: 'members' DbSet-ini sildik, çünki IdentityDbContext bunu avtomatik 'AspNetUsers' olaraq yaradır.

        public DbSet<Student> students { get; set; }
        public DbSet<Teacher> teachers { get; set; }
        public DbSet<Group> Groups { get; set; }
    }
}