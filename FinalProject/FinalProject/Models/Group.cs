using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models
{
    public class Group:BaseModel
    {
        [Required(ErrorMessage = "Qrup adı mütləq daxil edilməlidir.")]
        [StringLength(50)]
        public string GroupName { get; set; } // Məsələn: P324, 611.23 və s.

        // Bu qrupda oxuyan tələbələrin siyahısı (One-to-Many əlaqəsi üçün)
        // Bir qrupda çoxlu tələbə ola bilər
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
