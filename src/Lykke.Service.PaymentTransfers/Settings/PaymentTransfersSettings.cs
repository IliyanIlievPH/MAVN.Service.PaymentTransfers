using JetBrains.Annotations;

namespace Lykke.Service.PaymentTransfers.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class PaymentTransfersSettings
    {
        public DbSettings Db { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }

        public string PaymentTransfersAddress { get; set; }

        public string MasterWalletAddress { get; set; }

        public string DefaultCurrency { get; set; }

        public string TokenCurrencyCode { get; set; }

        public int MaxAcceptOrRejectTransactionInPbfRetryCount { get; set; }
    }
}
