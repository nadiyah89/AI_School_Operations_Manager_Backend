namespace SchoolOperations.Models
{
    public class AcademicPerformance
    {
        // Unique identifier for the academic performance record
        public int Id { get; set; }

        // Student associated with this academic record
        public int StudentId { get; set; }

        // Name of the subject
        public string Subject { get; set; } = string.Empty;

        // Name/type of the examination
        public string ExamName { get; set; } = string.Empty;

        // Marks obtained by the student
        public decimal MarksObtained { get; set; }

        // Maximum marks for the examination
        public decimal MaximumMarks { get; set; }

        // Date of the examination
        public DateTime ExamDate { get; set; }

        // Indicates whether this academic record is active
        public bool IsActive { get; set; } = true;

        // Navigation property to the student
        public Student Student { get; set; } = null!;
    }
}