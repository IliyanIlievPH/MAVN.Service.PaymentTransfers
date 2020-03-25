using System;
using Falcon.Numerics;

namespace Lykke.Service.PaymentTransfers.Contract
{
    /// <summary>
    /// Request which is raised when a payment transfer is processed (accepted/rejected)
    /// </summary>
    public class PaymentTransferProcessedEvent
    {
        /// <summary>
        /// Id of the payment transfer
        /// </summary>
        public string TransferId { get; set; }

        /// <summary>
        /// Id of the customer
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Id of the campaign (burn rule)
        /// </summary>
        public string CampaignId { get; set; }

        /// <summary>
        /// Id of the invoice
        /// </summary>
        public string InvoiceId { get; set; }

        /// <summary>
        /// Amount of tokens paid
        /// </summary>
        public Money18 Amount { get; set; }

        /// <summary>
        /// Status of the request
        /// </summary>
        public ProcessedPaymentTransferStatus Status { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Name of the Instalment
        /// </summary>
        public string InstalmentName { get; set; }

        /// <summary>
        /// The Location Code from sf
        /// </summary>
        public string LocationCode { get; set; }
    }
}
