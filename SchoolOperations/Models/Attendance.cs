namespace SchoolOperations.Models
{
    public class Attendance
    {
        // Unique identifier for the attendance record
        public int Id { get; set; }

        // Foreign key connecting attendance to Student
        public int StudentId { get; set; }

        // Date of the attendance record
        public DateTime Date { get; set; }

        // True = Present
        // False = Absent
        public bool IsPresent { get; set; }

        // Indicates whether the attendance record is active
        public bool IsActive { get; set; } = true;

        // Navigation property to Student
        public Student? Student { get; set; }
    }
}