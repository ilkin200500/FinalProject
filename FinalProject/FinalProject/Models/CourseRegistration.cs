using System;

namespace FinalProject.Models
{
    public class CourseRegistration
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public string Semester { get; set; } = "2026 Payız";
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
    }
}