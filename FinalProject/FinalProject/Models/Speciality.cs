using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models
{
    public class Speciality : BaseModel
    {
        [Required(ErrorMessage = "İxtisas adı mütləq daxil edilməlidir.")]
        [StringLength(100, ErrorMessage = "İxtisas adı maksimum 100 simvol ola bilər.")]
        public string Name { get; set; } // Məs: "Kompüter Mühəndisliyi"

        // Əlaqələr (Navigation Properties)
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}