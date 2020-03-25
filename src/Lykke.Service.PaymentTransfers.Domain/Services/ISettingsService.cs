namespace Lykke.Service.PaymentTransfers.Domain.Services
{
    public interface ISettingsService
    {
        string GetPaymentTransfersAddress();
        string GetMasterWalletAddress();
        string GetDefaultCurrency();
        string GetTokenCurrencyCode();
        int GetMaxAcceptOrRejectTransactionInPbfRetryCount();
    }
}
