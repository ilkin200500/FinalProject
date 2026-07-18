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

        // ➕ Yeni İxtisas Əlaqəsi (Bu fənn hansı ixtisasa aiddir?)
        [Required(ErrorMessage = "Zəhmət olmasa bu fənnin aid olduğu ixtisası seçin.")]
        public int SpecialityId { get; set; }
        public virtual Speciality? Speciality { get; set; }

        // 🎯 DÜZƏLİŞ: Birbaşa müəllim əlaqəsini sildik.
        // Onun əvəzinə bu fənnin hansı dərslərdə (Schedule) istifadə olunduğunu izləmək üçün cədvəl kolleksiyası əlavə edirik.
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

        // Entity Framework üçün naviqasiya property-si
        // Bu fənn üzrə yazılan bütün qiymətlərin siyahısı
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}