namespace SchoolOperations.DTOs.FeeRecord
{
    public class UpdateFeeRecordDto
    {
        // Type of fee
        public string FeeType { get; set; } = string.Empty;

        // Total fee amount
        public decimal Amount { get; set; }

        // Date by which the fee should be paid
        public DateTime DueDate { get; set; }

        // Amount already paid
        public decimal PaidAmount { get; set; }

        // Date on which payment was made
        public DateTime? PaymentDate { get; set; }
    }
}