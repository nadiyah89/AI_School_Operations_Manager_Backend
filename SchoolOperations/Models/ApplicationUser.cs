using Microsoft.AspNetCore.Identity;

namespace SchoolOperations.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Links the login account to a Student record
        // Used when this user has the Student role
        public int? StudentId { get; set; }

        // Links the login account to a Teacher record
        // Used when this user has the Teacher role
        public int? TeacherId { get; set; }

        // Links the login account to a Parent record
        // Used when this user has the Parent role
        public int? ParentId { get; set; }

        // Navigation properties
        public Student? Student { get; set; }

        public Teacher? Teacher { get; set; }

        public Parent? Parent { get; set; }
    }
}