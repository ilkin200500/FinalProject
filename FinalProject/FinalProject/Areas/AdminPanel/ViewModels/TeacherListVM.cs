using System.Collections.Generic;
using FinalProject.Models; // Sizin Teacher modelinin olduğu yer

namespace FinalProject.ViewModels
{
    public class TeacherListVM
    {
        // İstifadəçinin axtarış qutusuna yazdığı mətn
        public string SearchText { get; set; }

        // Tapılan müəllimlərin siyahısı
        public List<Teacher> Teachers { get; set; }
    }
}