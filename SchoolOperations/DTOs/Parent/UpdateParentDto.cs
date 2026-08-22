namespace SchoolOperations.DTOs.Parent
{
    public class UpdateParentDto
    {
        // Parent's basic information
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // Parent's contact information
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Relationship with the student
        public string Relationship { get; set; } = string.Empty;
    }
}