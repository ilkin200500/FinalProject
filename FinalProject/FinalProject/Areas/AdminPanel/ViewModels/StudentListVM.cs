using System.Collections.Generic;
using FinalProject.Models; // Sizin Student modelinin olduğu yer

namespace FinalProject.ViewModels
{
    public class StudentListVM
    {
        // İstifadəçinin axtarış qutusuna yazdığı mətn
        public string SearchText { get; set; }

        // Tapılan tələbələrin siyahısı
        public List<Student> Students { get; set; }
    }
}