using System.ComponentModel.DataAnnotations;

namespace FinalProject.ViewModels
{
    public class CreateCourseVM
    {
        [Required(ErrorMessage = "Fənnin adı mütləq daxil edilməlidir!")]
        [StringLength(100, ErrorMessage = "Fənnin adı çox uzundur.")]
        public string CourseName { get; set; }

        [Required(ErrorMessage = "Fənn kodu mütləqdir!")]
        [StringLength(10, ErrorMessage = "Fənn kodu maksimum 10 simvol ola bilər.")]
        public string CourseCode { get; set; }

        [Required(ErrorMessage = "Kredit miqdarı qeyd olunmalıdır!")]
        [Range(1, 10, ErrorMessage = "Kredit 1 ilə 10 arasında olmalıdır!")]
        public int Credits { get; set; }

        [Required(ErrorMessage = "Zəhmət olmasa, fənn üçün bir ixtisas seçin!")]
        [Range(1, int.MaxValue, ErrorMessage = "Zəhmət olmasa, etibarlı bir ixtisas seçin!")]
        public int SpecialityId { get; set; }
    }
}