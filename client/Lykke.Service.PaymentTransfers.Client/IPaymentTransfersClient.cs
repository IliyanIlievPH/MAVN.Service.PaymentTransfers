using JetBrains.Annotations;

namespace Lykke.Service.PaymentTransfers.Client
{
    /// <summary>
    /// PaymentTransfers client interface.
    /// </summary>
    [PublicAPI]
    public interface IPaymentTransfersClient
    {
        // Make your app's controller interfaces visible by adding corresponding properties here.
        // NO actual methods should be placed here (these go to controller interfaces, for example - IPaymentTransfersApi).
        // ONLY properties for accessing controller interfaces are allowed.

        /// <summary>Application Api interface</summary>
        IPaymentTransfersApi Api { get; }
    }
}
