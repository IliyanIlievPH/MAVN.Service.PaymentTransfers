using Lykke.HttpClientGenerator;

namespace MAVN.Service.PaymentTransfers.Client
{
    /// <summary>
    /// PaymentTransfers API aggregating interface.
    /// </summary>
    public class PaymentTransfersClient : IPaymentTransfersClient
    {
        // Note: Add similar Api properties for each new service controller

        /// <summary>Inerface to PaymentTransfers Api.</summary>
        public IPaymentTransfersApi Api { get; private set; }

        /// <summary>C-tor</summary>
        public PaymentTransfersClient(IHttpClientGenerator httpClientGenerator)
        {
            Api = httpClientGenerator.Generate<IPaymentTransfersApi>();
        }
    }
}
