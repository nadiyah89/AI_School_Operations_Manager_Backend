namespace SchoolOperations.DTOs.Admission
{
    public class CreateParentFromAdmissionDto
    {
        // Parent's first name
        public string FirstName { get; set; } = string.Empty;

        // Parent's last name
        public string LastName { get; set; } = string.Empty;

        // Parent's contact information
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Relationship with the student
        public string Relationship { get; set; } = string.Empty;
    }
}