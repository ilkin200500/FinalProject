using System;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models
{
    public class Assignment : BaseModel
    {
        [Required(ErrorMessage = "Tapşırığın başlığı mütləq yazılmalıdır.")]
        [StringLength(200)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Tapşırıq haqqında məlumat yazılmalıdır.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Son təhvil tarixi seçilməlidir.")]
        public DateTime DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Keys
        [Required]
        public int CourseId { get; set; }
        public virtual Course? Course { get; set; }

        [Required]
        public int GroupId { get; set; }
        public virtual Group? Group { get; set; }

        [Required]
        public int TeacherId { get; set; }
        public virtual Teacher? Teacher { get; set; }
    }
}