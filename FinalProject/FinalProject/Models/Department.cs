using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models
{
    public class Department:BaseModel
    {
        [Required(ErrorMessage = "Kafedra adı mütləqdir.")]
        [StringLength(100)]
        public string DepartmentName { get; set; }

        // Bu kafedraya bağlı olan müəllimlərin siyahısı
        public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    }
}
