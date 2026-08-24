namespace SchoolOperations.DTOs.Document
{
    public class UpdateDocumentDto
    {
        // Updated document title
        public string Title { get; set; } = string.Empty;

        // Updated document category
        public string Category { get; set; } = string.Empty;

        // Updated document content
        public string Content { get; set; } = string.Empty;
    }
}