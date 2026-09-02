namespace SchoolOperations.DTOs.Admission
{
    public class AdmissionSummaryDto
    {
        // Total number of admission applications
        public int TotalApplications { get; set; }

        // Number of applications currently pending
        public int PendingApplications { get; set; }

        // Number of approved applications
        public int ApprovedApplications { get; set; }

        // Number of rejected applications
        public int RejectedApplications { get; set; }

        // Number of applications currently on the waiting list
        public int WaitlistedApplications { get; set; }
    }
}