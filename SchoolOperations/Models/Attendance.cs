namespace SchoolOperations.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        // ID of the student this attendance belongs to
        public int StudentId { get; set; }

        // Date of the attendance record
        public DateTime Date { get; set; }

        // Whether the student was present
        public bool IsPresent { get; set; }

        // Navigation property to the related student
        public Student? Student { get; set; }
    }
}