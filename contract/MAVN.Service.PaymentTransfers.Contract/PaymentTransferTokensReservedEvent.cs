using System;
using Falcon.Numerics;

namespace MAVN.Service.PaymentTransfers.Contract
{
    /// <summary>
    /// Event which is raised when tokens for a payment transfer are reserved in BC
    /// </summary>
    public class PaymentTransferTokensReservedEvent
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
        /// Id of the invoice
        /// </summary>
        public string InvoiceId { get; set; }

        /// <summary>
        /// Id of the campaign (burn rule)
        /// </summary>
        public string CampaignId { get; set; }

        /// <summary>
        /// Amount of tokens paid
        /// </summary>
        public Money18 Amount { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Name of the instalment
        /// </summary>
        public string InstalmentName { get; set; }

        /// <summary>
        /// The Location Code from sf
        /// </summary>
        public string LocationCode { get; set; }
    }
}
