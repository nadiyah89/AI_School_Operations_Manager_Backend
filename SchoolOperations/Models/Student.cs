namespace SchoolOperations.Models
{
    public class Student
    {
        // Unique identifier for the student
        public int Id { get; set; }

        // Student's first name
        public string FirstName { get; set; } = string.Empty;

        // Student's last name
        public string LastName { get; set; } = string.Empty;

        // Student's date of birth
        public DateTime DateOfBirth { get; set; }

    }
}
