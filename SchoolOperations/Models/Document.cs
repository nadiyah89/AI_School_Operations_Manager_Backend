namespace SchoolOperations.Models
{
    public class Document
    {
        // Unique identifier for the document
        public int Id { get; set; }

        // Name/title of the document
        public string Title { get; set; } = string.Empty;

        // Category of the document
        // Examples: Policy, Admission, Academic, Attendance
        public string Category { get; set; } = string.Empty;

        // Main text/content of the document
        public string Content { get; set; } = string.Empty;

        // Date and time when the document was created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Date and time when the document was last updated
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Indicates whether the document is currently active
        public bool IsActive { get; set; } = true;
    }
}