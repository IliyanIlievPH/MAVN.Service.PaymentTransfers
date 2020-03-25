using Lykke.Service.PaymentTransfers.Domain.Services;

namespace Lykke.Service.PaymentTransfers.DomainServices
{
    public class SettingsService : ISettingsService
    {
        private readonly string _paymentTransfersAddress;
        private readonly string _masterWalletAddress;
        private readonly string _defaultCurrency;
        private readonly string _tokenCurrencyCode;
        private readonly int _maxAcceptOrRejectTransactionInPbfRetryCount;

        public SettingsService(
            string paymentTransfersAddress,
            string masterWalletAddress,
            string defaultCurrency,
            string tokenCurrencyCode,
            int maxAcceptOrRejectTransactionInPbfRetryCount)
        {
            _paymentTransfersAddress = paymentTransfersAddress;
            _masterWalletAddress = masterWalletAddress;
            _defaultCurrency = defaultCurrency;
            _tokenCurrencyCode = tokenCurrencyCode;
            _maxAcceptOrRejectTransactionInPbfRetryCount = maxAcceptOrRejectTransactionInPbfRetryCount;
        }

        public string GetPaymentTransfersAddress()
            => _paymentTransfersAddress;

        public string GetMasterWalletAddress()
            => _masterWalletAddress;

        public string GetDefaultCurrency()
            => _defaultCurrency;

        public string GetTokenCurrencyCode()
            => _tokenCurrencyCode;

        public int GetMaxAcceptOrRejectTransactionInPbfRetryCount()
            => _maxAcceptOrRejectTransactionInPbfRetryCount;
    }

}
