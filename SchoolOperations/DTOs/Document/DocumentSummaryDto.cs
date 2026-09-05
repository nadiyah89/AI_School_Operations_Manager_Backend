namespace SchoolOperations.DTOs.Document
{
    public class DocumentSummaryDto
    {
        public int Id { get; set; }

        public string Title { get; set; }
            = string.Empty;

        public string Category { get; set; }
            = string.Empty;

        public DateTime UpdatedAt { get; set; }
    }
}