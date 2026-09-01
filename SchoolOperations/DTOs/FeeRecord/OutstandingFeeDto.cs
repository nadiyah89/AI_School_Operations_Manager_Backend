namespace SchoolOperations.DTOs.FeeRecord
{
    public class OutstandingFeeDto
    {
        // Unique identifier of the fee record
        public int FeeRecordId { get; set; }

        // Student associated with this fee record
        public int StudentId { get; set; }

        // Student name for displaying the analytics result
        public string StudentName { get; set; } = string.Empty;

        // Type of fee
        public string FeeType { get; set; } = string.Empty;

        // Total fee amount
        public decimal Amount { get; set; }

        // Amount already paid
        public decimal PaidAmount { get; set; }

        // Amount still owed
        // This value will be calculated by the backend
        public decimal OutstandingAmount { get; set; }

        // Date by which the fee should be paid
        public DateTime DueDate { get; set; }

        // Current payment status
        public string Status { get; set; } = string.Empty;
    }
}