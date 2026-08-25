namespace SchoolOperations.Models
{
    public class FeeRecord
    {
        // Unique identifier for the fee record
        public int Id { get; set; }

        // Student associated with this fee
        public int StudentId { get; set; }

        // Type of fee
        public string FeeType { get; set; } = string.Empty;

        // Total amount that needs to be paid
        public decimal Amount { get; set; }

        // Date by which the fee should be paid
        public DateTime DueDate { get; set; }

        // Amount already paid
        public decimal PaidAmount { get; set; }

        // Date on which payment was made
        public DateTime? PaymentDate { get; set; }

        // Current payment status
        public string Status { get; set; } = "Pending";

        // Indicates whether this fee record is active
        public bool IsActive { get; set; } = true;

        // Navigation property to Student
        public Student? Student { get; set; }
    }
}