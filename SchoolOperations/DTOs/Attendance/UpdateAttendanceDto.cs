namespace SchoolOperations.DTOs.Attendance
{
    public class UpdateAttendanceDto
    {
        // Attendance date
        public DateTime Date { get; set; }

        // Whether the student was present
        public bool IsPresent { get; set; }
    }
}