namespace Lykke.Service.PaymentTransfers.Domain.Enums
{
    public enum PaymentTransfersErrorCodes
    {
        None,
        NotEnoughFunds,
        InvalidSpendRuleId,
        SpendRuleNotFound,
        InvalidVerticalInSpendRule,
        CustomerDoesNotExist,
        CustomerWalletBlocked,
        PaymentTransferNotFound,
        InvalidStatus,
        CustomerWalletDoesNotExist,
        PaymentTransferAlreadyProcessing,
        CannotPassBothFiatAndTokensAmount,
        EitherFiatOrTokensAmountShouldBePassed,
        InvalidTokensAmount,
        InvalidFiatAmount,
        InvalidAmountConversion,
    }
}
