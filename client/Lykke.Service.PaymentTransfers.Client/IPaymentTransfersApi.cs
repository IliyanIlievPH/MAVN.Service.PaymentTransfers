using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.PaymentTransfers.Client.Enums;
using Lykke.Service.PaymentTransfers.Client.Models.Requests;
using Lykke.Service.PaymentTransfers.Client.Models.Responses;
using Refit;

namespace Lykke.Service.PaymentTransfers.Client
{
    // This is an example of service controller interfaces.
    // Actual interface methods must be placed here (not in IPaymentTransfersClient interface).

    /// <summary>
    /// PaymentTransfers client API interface.
    /// </summary>
    [PublicAPI]
    public interface IPaymentTransfersApi
    {
        /// <summary>
        /// Transfer which is used when a customer wants to spend tokens on a campaign
        /// </summary>
        /// <returns><see cref="PaymentTransferResponse"/></returns>
        [Post("/api/payments")]
        Task<PaymentTransferResponse> PaymentTransferAsync(PaymentTransferRequest transferRequest);
    }
}
