using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models
{
    public class Student : BaseModel
    {
        [Required(ErrorMessage = "Ad və Soyad mütləq daxil edilməlidir.")]
        public string FullName { get; set; }

        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        public int Average { get; set; }

        [Required]
        public int MemberId { get; set; }

        [Required]
        public string StudentCode { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Gender { get; set; }

        // --- ƏLAQƏLƏR ---

        [Required(ErrorMessage = "Zəhmət olmasa ixtisası seçin.")]
        public int SpecialityId { get; set; }
        public virtual Speciality? Speciality { get; set; }

        [Required(ErrorMessage = "Zəhmət olmasa qrupu seçin.")]
        public int GroupId { get; set; }

        public virtual Group? Group { get; set; }

        // Tələbənin bildirişlərinin siyahısı (One-to-Many)
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        // 🎯 YENİ ƏLAVƏ: Tələbənin seçdiyi fənlərin siyahısı (Many-to-Many əlaqə keçidi)
        // Nullable (?) etdik ki, yeni tələbə qeydiyyatdan keçəndə validasiya problemi yaratmasın
        public virtual ICollection<StudentSubject>? StudentSubjects { get; set; } = new List<StudentSubject>();
    }
}