namespace SchoolOperations.Models
{
    public class Teacher
    {
        // Unique identifier for the teacher
        public int Id { get; set; }

        // Teacher's first name
        public string FirstName { get; set; } = string.Empty;

        // Teacher's last name
        public string LastName { get; set; } = string.Empty;

        // Teacher's phone number
        public string PhoneNumber { get; set; } = string.Empty;

        // Teacher's email address
        public string Email { get; set; } = string.Empty;

        // Indicates whether the teacher is currently active
        public bool IsActive { get; set; } = true;
    }
}