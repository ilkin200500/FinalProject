using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models
{
    public class Member:BaseModel
    {
        
        [Required]
        public string FullName { get; set; }
        [Required,DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        

    }
}
