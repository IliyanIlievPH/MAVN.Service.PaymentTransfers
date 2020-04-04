using System;
using Falcon.Numerics;
using MAVN.Service.PaymentTransfers.Client.Enums;

namespace MAVN.Service.PaymentTransfers.Client.Models.Responses
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
