namespace SchoolOperations.DTOs.AcademicPerformance
{
    public class DecliningPerformanceSummaryDto
    {
        // Student identifier
        public int StudentId { get; set; }

        // Student name for displaying results
        public string StudentName { get; set; } = string.Empty;

        // Subject being analyzed
        public string Subject { get; set; } = string.Empty;


        // Previous examination details
        public string PreviousExamName { get; set; } = string.Empty;

        public DateTime PreviousExamDate { get; set; }

        public decimal PreviousPercentage { get; set; }


        // Latest examination details
        public string LatestExamName { get; set; } = string.Empty;

        public DateTime LatestExamDate { get; set; }

        public decimal LatestPercentage { get; set; }


        // LatestPercentage - PreviousPercentage
        // A negative value means performance declined
        public decimal PercentageChange { get; set; }
    }
}