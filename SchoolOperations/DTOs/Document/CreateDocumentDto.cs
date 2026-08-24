namespace SchoolOperations.DTOs.Document
{
    public class CreateDocumentDto
    {
        // Name/title of the document
        public string Title { get; set; } = string.Empty;

        // Category of the document
        // Examples: Policy, Attendance, Admission, Academic
        public string Category { get; set; } = string.Empty;

        // Main content of the document
        public string Content { get; set; } = string.Empty;
    }
}