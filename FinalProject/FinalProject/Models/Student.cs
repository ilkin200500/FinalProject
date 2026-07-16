using System;
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
        public string Speciality { get; set; }  // İxtisas

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
        [Required(ErrorMessage = "Zəhmət olmasa qrupu seçin.")]
        public int GroupId { get; set; }

        // Validation xətası verməməsi üçün nullable (?) etdik. 
        // Çünki tələbə yaradılan zaman bu obyekt deyil, yalnız id-si (GroupId) göndərilir.
        public Group? Group { get; set; }
        // Tələbənin bildirişlərinin siyahısı (One-to-Many)
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}