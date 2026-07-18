using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FinalProject.Models
{
    public class Group : BaseModel
    {
        [Required(ErrorMessage = "Qrup adı mütləq daxil edilməlidir.")]
        [StringLength(50)]
        public string GroupName { get; set; }

        // 🎯 int? (Nullable) edirik ki, köhnə qruplar üçün xəta verməsin
        public int? SpecialityId { get; set; }
        public Speciality Speciality { get; set; }

        // Bu qrupda oxuyan tələbələrin siyahısı
        public ICollection<Student> Students { get; set; } = new List<Student>();

        // YENİ ƏLAVƏ: Bu qrupun dərs cədvəlinin siyahısı (One-to-Many)
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}