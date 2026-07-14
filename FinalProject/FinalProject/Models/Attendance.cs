using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public bool IsPresent { get; set; }

        // --- Əlaqələr (Foreign Keys) ---

        [Required]
        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public virtual Student Student { get; set; }

        [Required]
        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        // 🎯 DƏYİŞƏN HİSSƏ: int-i 'int?' etdik və Teacher-i də nullable etdik
        [ForeignKey("Teacher")]
        public int? TeacherId { get; set; } // 👈 Sual işarəsi '?' mütləqdir, bu onun null (boş) ola biləcəyini göstərir
        public virtual Teacher? Teacher { get; set; }
    }
}