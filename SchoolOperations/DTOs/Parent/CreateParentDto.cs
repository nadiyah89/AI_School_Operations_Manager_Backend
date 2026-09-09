using System.ComponentModel.DataAnnotations;

namespace SchoolOperations.DTOs.Parent
{
    public class CreateParentDto
    {
        // Parent's basic information
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // Parent's contact information
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Relationship with the student
        public string Relationship { get; set; } = string.Empty;

        // Student that this parent belongs to
        public int StudentId { get; set; }
    }
}