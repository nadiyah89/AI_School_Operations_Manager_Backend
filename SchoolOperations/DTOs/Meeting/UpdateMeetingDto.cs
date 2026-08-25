namespace SchoolOperations.DTOs.Meeting
{
    public class UpdateMeetingDto
    {
        // Date and time of the meeting
        public DateTime MeetingDate { get; set; }

        // Purpose of the meeting
        public string Purpose { get; set; } = string.Empty;

        // Optional notes about the meeting
        public string? Notes { get; set; }

        // Current status of the meeting
        public string Status { get; set; } = string.Empty;
    }
}