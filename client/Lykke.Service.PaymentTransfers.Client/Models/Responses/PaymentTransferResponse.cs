using System;
using Falcon.Numerics;
using Lykke.Service.PaymentTransfers.Client.Enums;

namespace Lykke.Service.PaymentTransfers.Client.Models.Responses
{
    /// <summary>
    /// response model
    /// </summary>
    public class PaymentTransferResponse
    {
        /// <summary>
        /// error code
        /// </summary>
        public PaymentTransfersErrorCodes Error { get; set; }
    }
}
