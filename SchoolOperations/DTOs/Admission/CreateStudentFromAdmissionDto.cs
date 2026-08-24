namespace SchoolOperations.DTOs.Admission
{
    public class CreateStudentFromAdmissionDto
    {
        // Allows the school to confirm or correct the student's name
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }
    }
}