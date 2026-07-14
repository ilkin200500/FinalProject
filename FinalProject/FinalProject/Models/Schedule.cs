using System;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models
{
    public class Schedule : BaseModel
    {
        // 1. Fənn ilə Əlaqə (Foreign Key)
        [Required(ErrorMessage = "Fənn seçilməlidir.")]
        public int CourseId { get; set; }
        public Course Course { get; set; }

        // 2. Qrup ilə Əlaqə (Foreign Key)
        [Required(ErrorMessage = "Qrup seçilməlidir.")]
        public int GroupId { get; set; }
        public Group Group { get; set; } // Real Group modelinə keçid

        // 3. Müəllim ilə Əlaqə (Foreign Key) - YENİ ƏLAVƏ EDİLDİ 🎯
        [Required(ErrorMessage = "Müəllim seçilməlidir.")]
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }

        [Required(ErrorMessage = "Həftənin günü seçilməlidir.")]
        public DayOfWeek DayOfWeek { get; set; }

        [Required(ErrorMessage = "Dərsin başlama saatı mütləqdir.")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Dərsin bitmə saatı mütləqdir.")]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Auditoriya qeyd edilməlidir.")]
        public string Classroom { get; set; }
    }
}