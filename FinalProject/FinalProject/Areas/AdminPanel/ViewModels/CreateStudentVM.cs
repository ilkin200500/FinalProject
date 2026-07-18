using System;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Areas.AdminPanel.ViewModels
{
    public class CreateStudentVM
    {
        [Required(ErrorMessage = "Ad və Soyad mütləqdir.")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email mütləqdir.")]
        [EmailAddress(ErrorMessage = "Düzgün bir email ünvanı daxil edin.")]
        public string Email { get; set; }

        // 🎯 DÜZƏLİŞ: Köhnə string Speciality silindi, Dropdown-dan gələcək ID üçün bu sahə əlavə olundu:
        [Required(ErrorMessage = "Zəhmət olmasa ixtisası seçin.")]
        public int SpecialityId { get; set; }

        [Required(ErrorMessage = "Ortalama bal mütləqdir.")]
        [Range(0, 100, ErrorMessage = "Bal 0-100 arası olmalıdır.")]
        public int Average { get; set; }

        [Required(ErrorMessage = "İstifadəçi ID (MemberId) mütləqdir.")]
        public int MemberId { get; set; }

        [Required(ErrorMessage = "Tələbə kodu mütləqdir.")]
        public string StudentCode { get; set; }

        [Required(ErrorMessage = "Doğum tarixi mütləqdir.")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Telefon nömrəsi mütləqdir.")]
        [Phone(ErrorMessage = "Düzgün telefon nömrəsi daxil edin.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Cins seçilməlidir.")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Qrup seçilməlidir.")]
        public int GroupId { get; set; } // Dropdown-dan seçilən qrupun ID-si bura gələcək
    }
}