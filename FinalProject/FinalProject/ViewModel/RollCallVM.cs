using System;
using System.Collections.Generic;

namespace FinalProject.ViewModels // 👈 Qovluğunun adına uyğun olaraq dəyişdirildi
{
    public class RollCallVM
    {
        public int CourseId { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
        public List<StudentAttendanceSelection> Students { get; set; } = new List<StudentAttendanceSelection>();
    }

    public class StudentAttendanceSelection
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public bool IsPresent { get; set; }
    }
}