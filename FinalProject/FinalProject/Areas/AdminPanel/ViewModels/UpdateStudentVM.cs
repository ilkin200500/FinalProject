
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Areas.AdminPanel.ViewModels
{
    public class UpdateStudentVM
    {

        
        [Required]
        public string FullName { get; set; }
        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        public string Speciality { get; set; }  //ixtisas
        [Required]
        public int Average { get; set; }
    }
}
