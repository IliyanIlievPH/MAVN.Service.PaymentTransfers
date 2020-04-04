namespace MAVN.Service.PaymentTransfers.Client.Enums
{
    /// <summary>
    /// Holds error codes
    /// </summary>
    public enum ErrorCodes
    {
        /// <summary>
        /// No errors
        /// </summary>
        None,
        /// <summary>
        /// Payment transfer was not found
        /// </summary>
        PaymentTransferNotFound,
        /// <summary>
        /// Trying to update payment transfer with/to invalid status
        /// </summary>
        InvalidStatus,
        /// <summary>
        /// There is no wallet for the provided customerId
        /// </summary>
        CustomerWalletDoesNotExist,
        /// <summary>
        /// The transfer is being processed by Blockchain already
        /// </summary>
        PaymentTransferAlreadyProcessing,
        /// <summary>
        /// The request to pay the invoice in SF failed
        /// </summary>
        SalesForceRequestFailed
    }
}
