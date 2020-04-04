using System;
using Falcon.Numerics;
using MAVN.Service.PaymentTransfers.Domain.Enums;

namespace MAVN.Service.PaymentTransfers.Domain.Models
{
    public class PaymentTransferDto
    {
        public string TransferId { get; set; }
        public string CustomerId { get; set; }
        public string SpendRuleId { get; set; }
        public string InvoiceId { get; set; }
        public Money18? AmountInTokens { get; set; }
        public decimal? AmountInFiat { get; set; }
        public string Currency { get; set; }
        public PaymentTransferStatus Status { get; set; }
        public DateTime Timestamp { get; set; }
        public string LocationCode { get; set; }
        public string CustomerAccountNumber { get; set; }
        public string CustomerTrxId { get; set; }
        public string InstallmentType { get; set; }
        public string ReceiptNumber { get; set; }
        public string OrgId { get; set; }
        public string InstalmentName { get; set; }
    }
}
