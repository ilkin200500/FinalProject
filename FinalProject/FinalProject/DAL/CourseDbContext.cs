using FinalProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.DAL
{
    public class CourseDbContext : IdentityDbContext<AppUser>
    {
        public CourseDbContext(DbContextOptions options) : base(options)
        {
        }

       public DbSet<Member>members { get; set; }
        public DbSet<Student> students { get; set; }
        public DbSet<Teacher> teachers { get; set; }
    }
}
