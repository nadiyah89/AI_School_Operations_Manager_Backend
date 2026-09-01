namespace SchoolOperations.DTOs.FeeRecord
{
    public class FeeSummaryDto
    {
        // Total number of active fee records included in the summary
        public int TotalFeeRecords { get; set; }

        // Total amount expected from all included fee records
        public decimal TotalFeeAmount { get; set; }

        // Total amount already paid across all included fee records
        public decimal TotalPaidAmount { get; set; }

        // Total amount still outstanding across all included fee records
        public decimal TotalOutstandingAmount { get; set; }

        // Number of fee records where nothing has been paid
        public int PendingFeeRecords { get; set; }

        // Number of fee records where some amount has been paid,
        // but the full amount has not yet been paid
        public int PartiallyPaidFeeRecords { get; set; }

        // Number of fully paid fee records
        public int PaidFeeRecords { get; set; }

        // Number of fee records past their due date
        // that still have an outstanding balance
        public int OverdueFeeRecords { get; set; }
    }
}