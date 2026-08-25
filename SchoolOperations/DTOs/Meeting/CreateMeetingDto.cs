namespace SchoolOperations.DTOs.Meeting
{
    public class CreateMeetingDto
    {
        // Student associated with the meeting
        public int StudentId { get; set; }

        // Teacher associated with the meeting
        public int TeacherId { get; set; }

        // Date and time of the meeting
        public DateTime MeetingDate { get; set; }

        // Purpose of the meeting
        public string Purpose { get; set; } = string.Empty;

        // Optional notes about the meeting
        public string? Notes { get; set; }
    }
}