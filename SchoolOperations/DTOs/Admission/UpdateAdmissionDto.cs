namespace SchoolOperations.DTOs.Admission
{
    public class UpdateAdmissionDto
    {
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
    }
}