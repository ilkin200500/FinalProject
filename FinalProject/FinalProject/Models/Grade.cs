namespace FinalProject.Models
{
    public class Grade
    {
        public int Id { get; set; }

        // İmtahan qiymətləri
        public int Mids { get; set; }  // Giris bali
        public int Final { get; set; } // Final imtahanı qiyməti
        public int Total { get; set; } // Yekun bal (Mids + Final və ya sizin hesablama faiziniz)
        public string LetterGrade { get; set; } // A, B, C, D, E, F

        // İlişkilər (Foreign Keys)
        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public string Semester { get; set; } // Məsələn: "2026 Payız", "2026 Yaz"
    }
}