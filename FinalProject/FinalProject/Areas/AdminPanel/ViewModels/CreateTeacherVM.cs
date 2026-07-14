using System.ComponentModel.DataAnnotations;

namespace FinalProject.Areas.AdminPanel.ViewModels
{
    public class CreateTeacherVM
    {
        [Required(ErrorMessage = "Ad və Soyad mütləq daxil edilməlidir.")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email ünvanı mütləqdir.")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Düzgün bir email ünvanı daxil edin.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifrə mütləqdir.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifrə ən az 6 simvoldan ibarət olmalıdır.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "İxtisas sahəsi mütləqdir.")]
        public string Speciality { get; set; }

        [Required(ErrorMessage = "Maaş qeyd edilməlidir.")]
        [Range(0, int.MaxValue, ErrorMessage = "Maaş mənfi ola bilməz.")]
        public int Salary { get; set; }

        // Seçilən kafedranın ID-si (Məcburi deyil)
        public int? DepartmentId { get; set; }

        // 🎯 DÜZƏLİŞ: 'CourseIds' siyahısı buradan tamamilə silindi!
    }
}