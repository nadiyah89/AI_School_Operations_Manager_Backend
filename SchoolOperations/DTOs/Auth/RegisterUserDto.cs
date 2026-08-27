namespace SchoolOperations.DTOs.Auth
{
    public class RegisterUserDto
    {
        // User's email address
        public string Email { get; set; }

        // Password for the new account
        public string Password { get; set; }

        // Role assigned to the account
        public string Role { get; set; }

        // Links the account to an existing Student
        // Used only when Role = Student
        public int? StudentId { get; set; }

        // Links the account to an existing Teacher
        // Used only when Role = Teacher
        public int? TeacherId { get; set; }

        // Links the account to an existing Parent
        // Used only when Role = Parent
        public int? ParentId { get; set; }
    }
}