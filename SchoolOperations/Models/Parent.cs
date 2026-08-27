namespace SchoolOperations.Models
{
    public class Parent
    {
        // Primary key
        public int Id { get; set; }

        // Parent's basic information
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // Contact information
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Relationship with the student
        public string Relationship { get; set; } = string.Empty;

        // Foreign key connecting Parent to Student
        public int StudentId { get; set; }

        // Indicates whether the parent record is active
        public bool IsActive { get; set; } = true;

        // Navigation property
        public Student Student { get; set; } = null!;
    }
}