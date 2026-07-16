using System;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models
{
    public class Notification : BaseModel
    {
        // 1. Hansı Tələbəyə aid olduğunu bildirən Foreign Key
        [Required]
        public int StudentId { get; set; }

        // Entity Framework üçün Tələbə naviqasiya xassəsi (Nullable etdik ki, yaradılan zaman validation xətası verməsin)
        public virtual Student? Student { get; set; }

        // 2. Bildirişin Məzmunu
        [Required(ErrorMessage = "Bildiriş başlığı mütləq daxil edilməlidir.")]
        [StringLength(150, ErrorMessage = "Başlıq ən çox 150 simvol ola bilər.")]
        public string Title { get; set; } // Məsələn: "Yeni Qiymət Yazıldı"

        [Required(ErrorMessage = "Bildiriş mətri mütləq daxil edilməlidir.")]
        public string Message { get; set; } // Məsələn: "Riyaziyyat fənnindən qiymətiniz daxil edildi."

        // 3. Statuslar və Tarix
        public bool IsRead { get; set; } = false; // Oxunub / Oxunmayıb

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Yaradılma tarixi

        // 4. Könüllü: Bildirişi hansı müəllimin göndərdiyini qeyd etmək üçün Foreign Key
        // Nullable (?) etdik ki, sabah sistem avtomatik bildiriş göndərəndə müəllim ID-si tələb olunmasın
        public int? TeacherId { get; set; }

        // Entity Framework üçün Müəllim naviqasiya xassəsi
        public virtual Teacher? Teacher { get; set; }
    }
}