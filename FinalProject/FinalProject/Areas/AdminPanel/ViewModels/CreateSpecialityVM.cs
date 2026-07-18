using System.ComponentModel.DataAnnotations;

namespace FinalProject.Areas.AdminPanel.ViewModels
{
    public class CreateSpecialityVM
    {
        [Required(ErrorMessage = "İxtisas adı mütləq daxil edilməlidir!")]
        [StringLength(100, ErrorMessage = "İxtisas adı maksimum 100 simvol ola bilər.")]
        public string Name { get; set; }
    }
}