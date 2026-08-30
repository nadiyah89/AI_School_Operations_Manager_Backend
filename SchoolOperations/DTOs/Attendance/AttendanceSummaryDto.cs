namespace SchoolOperations.DTOs.Attendance
{
    public class AttendanceSummaryDto
    {
        // Student identifier
        public int StudentId { get; set; }

        // Student's full name
        public string StudentName { get; set; } = string.Empty;

        // Number of attendance days considered
        public int TotalDays { get; set; }

        // Number of days the student was present
        public int PresentDays { get; set; }

        // Number of days the student was absent
        public int AbsentDays { get; set; }

        // Attendance percentage
        public decimal AttendancePercentage { get; set; }
    }
}