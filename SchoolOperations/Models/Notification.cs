namespace SchoolOperations.Models
{
    public class Notification
    {
        // Unique identifier for the notification
        public int Id { get; set; }

        // Student associated with the notification
        public int StudentId { get; set; }

        // Parent associated with the notification
        public int ParentId { get; set; }

        // Type of notification
        public string NotificationType { get; set; } = string.Empty;

        // Notification message
        public string Message { get; set; } = string.Empty;

        // Channel used to send the notification
        public string Channel { get; set; } = string.Empty;

        // Current status of the notification
        public string Status { get; set; } = "Pending";

        // Date and time when the notification was created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Date and time when the notification was sent
        public DateTime? SentAt { get; set; }

        // Indicates whether the notification record is active
        public bool IsActive { get; set; } = true;

        // Navigation property to Student
        public Student? Student { get; set; }

        // Navigation property to Parent
        public Parent? Parent { get; set; }
    }
}