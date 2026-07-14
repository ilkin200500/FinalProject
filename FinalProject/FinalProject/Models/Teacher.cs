using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models
{
    public class Teacher : BaseModel
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

        // 2. 🎯 DÜZƏLİŞ: Fənn asılılığını tamamilə silib, yerinə Schedule (Cədvəl) əlaqəsini qoyuruq.
        // Bu obyekt vasitəsilə müəllim öz cədvəlində ona təyin olunmuş dərsləri görə biləcək.
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}