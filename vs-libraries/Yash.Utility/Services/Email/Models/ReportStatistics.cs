namespace Yash.Utility.Services.Email.Models
{
    /// <summary>
    /// Statistics for email reports
    /// </summary>
    public class ReportStatistics
    {
        /// <summary>
        /// Total number of transactions processed
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Number of successful transactions
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Number of failed transactions
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// Success rate as percentage
        /// </summary>
        public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount * 100 : 0;

        /// <summary>
        /// Failure rate as percentage
        /// </summary>
        public double FailureRate => TotalCount > 0 ? (double)FailedCount / TotalCount * 100 : 0;
    }
}