namespace SchoolOperations.Models
{
    public class AdmissionApplication
    {
        // Primary key
        public int Id { get; set; }

        // Applicant's basic information
        public string ApplicantFirstName { get; set; } = string.Empty;
        public string ApplicantLastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }

        // Class the applicant is applying for
        public string ApplyingForClass { get; set; } = string.Empty;

        // Parent/guardian contact information
        public string ParentName { get; set; } = string.Empty;
        public string ParentPhoneNumber { get; set; } = string.Empty;
        public string ParentEmail { get; set; } = string.Empty;

        // Admission application information
        public DateTime ApplicationDate { get; set; }

        // Current status of the application
        public string Status { get; set; } = "Pending";

        // Student created from this admission application
        // Null until the admission is finalized
        public int? StudentId { get; set; }


        // Parent created from this admission application
        // Null until the parent/guardian is created
        public int? ParentId { get; set; }
    }
}