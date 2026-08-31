namespace SchoolOperations.DTOs.AcademicPerformance
{
    public class PoorPerformanceSummaryDto
    {
        // Student identifier
        public int StudentId { get; set; }

        // Student name for displaying results
        public string StudentName { get; set; } = string.Empty;

        // Subject being evaluated
        public string Subject { get; set; } = string.Empty;

        // Latest examination name
        public string LatestExamName { get; set; } = string.Empty;

        // Date of the latest examination
        public DateTime LatestExamDate { get; set; }

        // Performance percentage calculated by the backend
        public decimal LatestPercentage { get; set; }
    }
}