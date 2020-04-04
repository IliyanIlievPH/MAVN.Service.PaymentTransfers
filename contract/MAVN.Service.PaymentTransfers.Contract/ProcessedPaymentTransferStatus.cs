namespace MAVN.Service.PaymentTransfers.Contract
{
    /// <summary>
    /// Payment transfer statuses
    /// </summary>
    public enum ProcessedPaymentTransferStatus
    {
        /// <summary>
        /// The payment transfer was accepted/approved
        /// </summary>
        Accepted,
        /// <summary>
        /// The payment transfer was rejected
        /// </summary>
        Rejected
    }
}
