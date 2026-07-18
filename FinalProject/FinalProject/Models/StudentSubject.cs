using System;

namespace FinalProject.Models
{
    public class StudentSubject
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public virtual Student? Student { get; set; }

        // 🎯 DÜZƏLİŞ: Sizin layihədə modelin adı Course olduğu üçün buraları Course etdik
        public int CourseId { get; set; }
        public virtual Course? Course { get; set; }

        public DateTime SelectionDate { get; set; } = DateTime.Now;
    }
}