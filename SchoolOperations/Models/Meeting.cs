namespace SchoolOperations.Models
{
    public class Meeting
    {
        // Unique identifier for the meeting
        public int Id { get; set; }

        // Student associated with the meeting
        public int StudentId { get; set; }

        // Teacher associated with the meeting
        public int TeacherId { get; set; }

        // Date and time of the meeting
        public DateTime MeetingDate { get; set; }

        // Purpose or subject of the meeting
        public string Purpose { get; set; } = string.Empty;

        // Current status of the meeting
        public string Status { get; set; } = "Scheduled";

        // Optional notes about the meeting
        public string? Notes { get; set; }

        // Indicates whether the meeting record is active
        public bool IsActive { get; set; } = true;

        // Navigation property to Student
        public Student? Student { get; set; }

        // Navigation property to Teacher
        public Teacher? Teacher { get; set; }
    }
}