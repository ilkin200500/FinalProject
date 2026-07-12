using System;

namespace FinalProject.Models
{
    public class Grade
    {
        public int Id { get; set; }

        // İmtahan qiymətləri
        public int Mids { get; set; }  // Giriş balı (0 - 50 arası)

        public int? Final { get; set; } // Final balı (0 - 50 arası). Hələ imtahan olmayıbsa null qalır.

        // Yekun bal - Avtomatik hesablanan computed property (Read-only)
        public int Total
        {
            get
            {
                // Əgər final hələ yazılmayıbsa, yekun bal sadəcə giriş balına bərabər olur
                return Mids + (Final ?? 0);
            }
        }

        // Hərfi qiymət - Total-a görə avtomatik təyin olunur (Read-only)
        public string LetterGrade
        {
            get
            {
                if (Final == null) return "Gözləmədə"; // Final imtahanı hələ keçirilməyib

                if (Total >= 91) return "A";
                if (Total >= 81) return "B";
                if (Total >= 71) return "C";
                if (Total >= 61) return "D";
                if (Total >= 51) return "E";
                return "F"; // 51-dən aşağı kəsir
            }
        }

        // İlişkilər (Foreign Keys)
        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        // Qiyməti yazan müəllim
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }

        public string Semester { get; set; } // Məsələn: "2026 Payız", "2026 Yaz"

        // Qiymətin yaradılma/təyin edilmə tarixi
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}