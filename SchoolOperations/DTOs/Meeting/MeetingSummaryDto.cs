namespace SchoolOperations.DTOs.Meeting
{
    public class MeetingSummaryDto
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public int TeacherId { get; set; }

        public string TeacherName { get; set; } = string.Empty;

        public DateTime MeetingDate { get; set; }

        public string Purpose { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}