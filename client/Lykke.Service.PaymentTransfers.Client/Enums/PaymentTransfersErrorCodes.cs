namespace Lykke.Service.PaymentTransfers.Client.Enums
{
    /// <summary>
    /// Payment transfers error codes.
    /// </summary>
    public enum PaymentTransfersErrorCodes
    {
        /// <summary>
        /// No error
        /// </summary>
        None,
        /// <summary>
        /// customer does not have enough balance
        /// </summary>
        NotEnoughFunds,
        /// <summary>
        /// The provided campaign id is not valid
        /// </summary>
        InvalidSpendRuleId,
        /// <summary>
        /// Campaign not found
        /// </summary>
        SpendRuleNotFound,
        /// <summary>
        /// The vertical in the spend rule is not real estate
        /// </summary>
        InvalidVerticalInSpendRule,
        /// <summary>
        /// There is not customer with the provided customerId
        /// </summary>
        CustomerDoesNotExist,
        /// <summary>
        /// The wallet of the customer is blocked
        /// </summary>
        CustomerWalletBlocked,
        /// <summary>
        /// Payment transfer was not found
        /// </summary>
        PaymentTransferNotFound,
        /// <summary>
        /// Trying to update to invalid status
        /// </summary>
        InvalidStatus,
        /// <summary>
        /// Customer does not have a wallet
        /// </summary>
        CustomerWalletDoesNotExist,
        /// <summary>
        /// This payment transfer is in processing already
        /// </summary>
        PaymentTransferAlreadyProcessing,
        /// <summary>
        /// Only one of them should be passed
        /// </summary>
        CannotPassBothFiatAndTokensAmount,
        /// <summary>
        ///  One of them should be passed
        /// </summary>
        EitherFiatOrTokensAmountShouldBePassed,
        /// <summary>
        /// Tokens amount is not valid 
        /// </summary>
        InvalidTokensAmount,
        /// <summary>
        /// fiat amount is not valid
        /// </summary>
        InvalidFiatAmount,
        /// <summary>
        /// Error during conversion of amounts
        /// </summary>
        InvalidAmountConversion,
    }
}
