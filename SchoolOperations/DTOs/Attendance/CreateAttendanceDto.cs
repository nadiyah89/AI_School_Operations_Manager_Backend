namespace SchoolOperations.DTOs.Attendance
{
    public class CreateAttendanceDto
    {
        // Student whose attendance is being recorded
        public int StudentId { get; set; }

        // Attendance date
        public DateTime Date { get; set; }

        // True = Present
        // False = Absent
        public bool IsPresent { get; set; }
    }
}