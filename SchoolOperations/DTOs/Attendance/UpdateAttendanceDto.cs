namespace SchoolOperations.DTOs.Attendance
{
    public class UpdateAttendanceDto
    {
        // Attendance date
        public DateTime Date { get; set; }

        // True = Present
        // False = Absent
        public bool IsPresent { get; set; }
    }
}