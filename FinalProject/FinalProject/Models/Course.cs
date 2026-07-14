using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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


        // --- ƏLAVƏ OLUNAN ƏLAQƏLƏR (RELATIONSHIPS) ---

        // 🎯 DÜZƏLİŞ: Birbaşa müəllim əlaqəsini sildik.
        // Onun əvəzinə bu fənnin hansı dərslərdə (Schedule) istifadə olunduğunu izləmək üçün cədvəl kolleksiyası əlavə edirik.
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

        // Entity Framework üçün naviqasiya property-si
        // Bu fənn üzrə yazılan bütün qiymətlərin siyahısı
        public ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}