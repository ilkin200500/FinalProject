using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models
{
    public class Course:BaseModel
    {
        [Required(ErrorMessage = "Fənnin adı mütləqdir.")]
        [StringLength(100)]
        public string CourseName { get; set; } // Məs: Riyaziyyat, C# Proqramlaşdırma

        [Required(ErrorMessage = "Fənnin kodu mütləqdir.")]
        public string CourseCode { get; set; } // Məs: COMP201, MATH101

        [Required(ErrorMessage = "Kredit miqdarı qeyd edilməlidir.")]
        [Range(1, 10)]
        public int Credits { get; set; } // Fənnin kredit sayı (Məs: 4-6)

        // Əgər Müəllim klassı ilə Many-to-Many və ya One-to-Many əlaqə quracaqsınızsa:
        public int? TeacherId { get; set; }
        public Teacher Teacher { get; set; }
    }
}
