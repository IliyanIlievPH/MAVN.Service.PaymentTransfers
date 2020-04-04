namespace MAVN.Service.PaymentTransfers.Client.Enums
{
    /// <summary>
    /// Enum which holds all statuses of a payment transfers
    /// </summary>
    public enum PaymentTransferStatus
    {
        /// <summary>
        /// The transfer is not processed yet
        /// </summary>
        Pending,
        /// <summary>
        /// Request is being processed by BC
        /// </summary>
        Processing,
        /// <summary>
        /// The transfer is accepted(approved)
        /// </summary>
        Accepted,
        /// <summary>
        /// The transfer is rejected
        /// </summary>
        Rejected,
    }
}
