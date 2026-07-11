using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Areas.AdminPanel.ViewModels
{
    public class UpdateTeacherVM
    {
        // 🎯 Xətanı aradan qaldıran əsas sətir:
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad və Soyad mütləqdir.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email mütləqdir.")]
        [EmailAddress(ErrorMessage = "Düzgün email ünvanı daxil edin.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "İxtisas mütləqdir.")]
        public string Speciality { get; set; }

        [Required(ErrorMessage = "Maaş mütləqdir.")]
        public int Salary { get; set; }

        public int? DepartmentId { get; set; }

        public List<int> CourseIds { get; set; }
    }
}