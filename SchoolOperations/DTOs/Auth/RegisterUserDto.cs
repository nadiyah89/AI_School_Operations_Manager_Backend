using System.ComponentModel.DataAnnotations;

namespace SchoolOperations.DTOs.Auth
{
    public class RegisterUserDto
    {
        // User's email address
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Password for the new account
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        // Role assigned to the account
        [Required]
        public string Role { get; set; } = string.Empty;

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