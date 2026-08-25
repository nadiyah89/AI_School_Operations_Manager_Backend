namespace SchoolOperations.DTOs.Notification
{
    public class CreateNotificationDto
    {
        // Student associated with the notification
        public int StudentId { get; set; }

        // Parent who will receive the notification
        public int ParentId { get; set; }

        // Type of notification
        public string NotificationType { get; set; } = string.Empty;

        // Actual notification message
        public string Message { get; set; } = string.Empty;

        // Channel used to send the notification
        public string Channel { get; set; } = string.Empty;
    }
}