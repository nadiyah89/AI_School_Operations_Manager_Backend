namespace SchoolOperations.DTOs.Attendance
{
    public class CreateAttendanceDto
    {
        // The student whose attendance is being recorded
        public int StudentId { get; set; }

        // The date of attendance
        public DateTime Date { get; set; }

        // True = Present, False = Absent
        public bool IsPresent { get; set; }
    }
}