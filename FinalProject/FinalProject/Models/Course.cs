using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FinalProject.Models
{
    public class Course : BaseModel
    {
        [Required(ErrorMessage = "Fənnin adı mütləqdir.")]
        [StringLength(100, ErrorMessage = "Fənnin adı maksimum {1} simvol ola bilər.")]
        public string CourseName { get; set; } // Məs: Riyaziyyat, C# Proqramlaşdırma

        [Required(ErrorMessage = "Fənnin kodu mütləqdir.")]
        [StringLength(20, ErrorMessage = "Fənnin kodu maksimum {1} simvol ola bilər.")]
        public string CourseCode { get; set; } // Məs: COMP201, MATH101

        [Required(ErrorMessage = "Kredit miqdarı qeyd edilməlidir.")]
        [Range(1, 10, ErrorMessage = "Kredit 1 ilə 10 arasında olmalıdır.")]
        public int Credits { get; set; } // Fənnin kredit sayı (Məs: 4-6)

        // Müəllim ilə əlaqə (One-to-Many)
        // Hər fənnin bir müəllimi ola bilər (və ya hələ təyin olunmayıb - null)
        public int? TeacherId { get; set; }
        public Teacher Teacher { get; set; }

        // Entity Framework üçün naviqasiya property-si
        // Bu fənn üzrə yazılan bütün qiymətlərin siyahısı
        public ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}