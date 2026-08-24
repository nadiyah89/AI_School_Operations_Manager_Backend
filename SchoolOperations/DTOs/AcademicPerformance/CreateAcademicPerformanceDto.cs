namespace SchoolOperations.DTOs.AcademicPerformance
{
    public class CreateAcademicPerformanceDto
    {
        // Student associated with this academic record
        public int StudentId { get; set; }

        // Subject in which the student was evaluated
        public string Subject { get; set; } = string.Empty;

        // Name of the examination
        public string ExamName { get; set; } = string.Empty;

        // Marks obtained by the student
        public decimal MarksObtained { get; set; }

        // Maximum marks for the examination
        public decimal MaximumMarks { get; set; }

        // Date on which the examination took place
        public DateTime ExamDate { get; set; }
    }
}