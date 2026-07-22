using FinalProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.DAL
{
    public class CourseDbContext : IdentityDbContext<Member, IdentityRole<int>, int>
    {
        public CourseDbContext(DbContextOptions<CourseDbContext> options) : base(options)
        {
        }

        public DbSet<Student> students { get; set; }
        public DbSet<Teacher> teachers { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Department> departments { get; set; }
        public DbSet<Course> courses { get; set; }
        public DbSet<Grade> grades { get; set; }
        public DbSet<CourseRegistration> courseRegistrations { get; set; }
        public DbSet<Attendance> attendances { get; set; }
        public DbSet<Schedule> schedules { get; set; }
        public DbSet<Notification> notifications { get; set; }
        public DbSet<Speciality> specialities { get; set; }
        public DbSet<StudentSubject> StudentSubjects { get; set; }

        public DbSet<Assignment> assignments { get; set; }

        // 🎯 Cəncirvari silinmə (Cascade) xətasını həll edən konfiqurasiya metodu:
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Students və Specialities arasındakı əlaqədə Cascade silinməni ləğv edirik
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Speciality)
                .WithMany()
                .HasForeignKey(s => s.SpecialityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Courses və Specialities arasındakı əlaqədə Cascade silinməni ləğv edirik
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Speciality)
                .WithMany()
                .HasForeignKey(c => c.SpecialityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}