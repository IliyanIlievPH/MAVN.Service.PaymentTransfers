using System.ComponentModel.DataAnnotations;
using Falcon.Numerics;

namespace MAVN.Service.PaymentTransfers.Client.Models.Requests
{
    /// <summary>
    /// Class which holds information about payment transfer request
    /// </summary>
    public class PaymentTransferRequest
    {
        /// <summary>
        /// Id of the customer
        /// </summary>
        [Required]
        public string CustomerId { get; set; }
        /// <summary>
        /// Id of the spend rule
        /// </summary>
        [Required]
        public string SpendRuleId { get; set; }

        /// <summary>
        /// Amount to pay in tokens
        /// </summary>
        public Money18? AmountInTokens { get; set; }

        /// <summary>
        /// Amount to pay in fiat
        /// </summary>
        public decimal? AmountInFiat { get; set; }

        /// <summary>
        /// Currency code
        /// </summary>\
        [Required]
        public string Currency { get; set; }

        /// <summary>
        /// Name of the property
        /// </summary>
        [Required]
        public string LocationCode { get; set; }

        /// <summary>
        /// Customer account number in SF
        /// </summary>
        [Required]
        public string CustomerAccountNumber { get; set; }

        /// <summary>
        /// CustomerTrxId in SF
        /// </summary>
        [Required]
        public string CustomerTrxId { get; set; }

        /// <summary>
        /// InstallmentType in SF
        /// </summary>
        [Required]
        public string InstallmentType { get; set; }

        /// <summary>
        /// OrgId in SF
        /// </summary>
        [Required]
        public string OrgId { get; set; }

        /// <summary>
        /// Name of the instalment
        /// </summary>
        [Required]
        public string InstalmentName { get; set; }
    }
}
