namespace SchoolOperations.DTOs.Student
{
    public class CreateStudentDto
    {
        // Student's first name
        public string FirstName { get; set; } = string.Empty;

        // Student's last name
        public string LastName { get; set; } = string.Empty;

        // Student's date of birth
        public DateTime DateOfBirth { get; set; }
    }
}