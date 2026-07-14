using System;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.ViewModels
{
    public class CreateScheduleVM
    {
        [Required(ErrorMessage = "Fənn seçilməlidir.")]
        public int CourseId { get; set; }

        // DÜZƏLDİLDİ: Köhnə string GroupName silindi, yerinə dropdown-dan gələcək ID qoyuldu
        [Required(ErrorMessage = "Qrup seçilməlidir.")]
        public int GroupId { get; set; }

        // 🎯 YENİ ƏLAVƏ EDİLDİ: Cədvəldə dərsi keçəcək müəllimin ID-si
        [Required(ErrorMessage = "Müəllim mütləq seçilməlidir.")]
        public int TeacherId { get; set; }

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