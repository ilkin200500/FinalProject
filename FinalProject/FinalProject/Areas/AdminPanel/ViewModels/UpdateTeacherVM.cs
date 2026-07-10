using System.ComponentModel.DataAnnotations;
using FinalProject.Models;

namespace FinalProject.Areas.AdminPanel.ViewModels
{
    public class UpdateTeacherVM
    {
        [Required(ErrorMessage = "Ad və Soyad mütləq daxil edilməlidir.")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email ünvanı mütləqdir.")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Düzgün bir email ünvanı daxil edin.")]
        public string Email { get; set; }

        // Müəllimin sistemə daxil ola bilməsi üçün şifrə sahəsi
        [Required(ErrorMessage = "Şifrə mütləqdir.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifrə ən az 6 simvoldan ibarət olmalıdır.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "İxtisas sahəsi mütləqdir.")]
        public string Speciality { get; set; } // İxtisas (məs: Proqramlaşdırma)

        [Required(ErrorMessage = "Maaş qeyd edilməlidir.")]
        [Range(0, int.MaxValue, ErrorMessage = "Maaş mənfi ola bilməz.")]
        public int Salary { get; set; }

        [Required]
        public int MemberId { get; set; }


        // --- ƏLAVƏ OLUNAN ƏLAQƏLƏR (RELATIONSHIPS) ---

        // 1. Müəllimin aid olduğu Kafedra/Fakültə (İstəyə bağlı, amma idarəetmə üçün yaxşıdır)
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        // 2. Müəllimin tədris etdiyi fənlərin/dərslərin siyahısı
        // Bu obyekt vasitəsilə müəllim öz panelində hansı dərsləri keçdiyini görə biləcək
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
