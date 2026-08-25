namespace SchoolOperations.DTOs.Notification
{
    public class UpdateNotificationDto
    {
        // Type of notification
        public string NotificationType { get; set; } = string.Empty;

        // Notification message
        public string Message { get; set; } = string.Empty;

        // Channel used to send the notification
        public string Channel { get; set; } = string.Empty;

        // Current status of the notification
        public string Status { get; set; } = string.Empty;

        // Date and time when the notification was sent
        public DateTime? SentAt { get; set; }
    }
}