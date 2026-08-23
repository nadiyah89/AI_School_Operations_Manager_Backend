namespace SchoolOperations.DTOs.Teacher
{
    public class UpdateTeacherDto
    {
        // Teacher's first name
        public string FirstName { get; set; } = string.Empty;

        // Teacher's last name
        public string LastName { get; set; } = string.Empty;

        // Teacher's phone number
        public string PhoneNumber { get; set; } = string.Empty;

        // Teacher's email address
        public string Email { get; set; } = string.Empty;
    }
}