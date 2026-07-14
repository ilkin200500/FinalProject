namespace FinalProject.ViewModels
{
    public class AttendanceReportVM
    {
        // Fənnin adı (məs: "Riyaziyyat")
        public string CourseName { get; set; }

        // Keçirilən ümumi dərs saatı
        public int TotalClasses { get; set; }

        // Tələbənin aldığı qayıb (QB) sayısı
        public int AbsentCount { get; set; }

        // Dərsdə iştirak faizi (məs: 85)
        public int Percentage { get; set; }

        // Limit keçilibsə true, keçilməyibsə false (Kəsilmə statusu)
        public bool IsFailed { get; set; }
    }
}