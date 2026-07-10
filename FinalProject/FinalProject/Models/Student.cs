using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Diagnostics.HealthChecks;

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
        public int Average { get; set; } // int-dən double-a dəyişdik (kəsr qiymətlər üçün)

        [Required]
        public int MemberId { get; set; }

        [Required]
        public string StudentCode { get; set; } // StudentId-ni StudentCode etdik (Baza xətası verməsin deyə)

        [Required]
        public DateTime BirthDate { get; set; } // BirtDate-dəki hərf səhvi BirthDate olaraq düzəldildi

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Gender { get; set; }

        // --- ƏLAQƏLƏR ---
        [Required]
        public int GroupId { get; set; }
        public Group Group { get; set; }
    }
}
