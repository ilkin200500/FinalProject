using System.ComponentModel.DataAnnotations;

namespace FinalProject.Areas.AdminPanel.ViewModels
{
    public class CreateTeacherVM
    {
        [Required]
        public string FullName { get; set; }
        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        public string Speciality { get; set; }  //ixtisas
        [Required]
        public int Salary { get; set; }
    }
}
