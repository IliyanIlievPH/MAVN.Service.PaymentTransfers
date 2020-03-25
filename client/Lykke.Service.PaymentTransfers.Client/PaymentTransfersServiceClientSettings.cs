using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PaymentTransfers.Client 
{
    /// <summary>
    /// PaymentTransfers client settings.
    /// </summary>
    public class PaymentTransfersServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}
    }
}
