using System.ComponentModel.DataAnnotations;

namespace SchoolOperations.DTOs.Auth
{
    public class LoginDto
    {
        // User's email address
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // User's password
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }
}