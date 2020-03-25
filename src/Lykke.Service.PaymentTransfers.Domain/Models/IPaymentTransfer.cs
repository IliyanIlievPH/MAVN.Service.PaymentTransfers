using System;
using Falcon.Numerics;
using Lykke.Service.PaymentTransfers.Domain.Enums;

namespace Lykke.Service.PaymentTransfers.Domain.Models
{
    public interface IPaymentTransfer
    {
        string TransferId { get; set; }
        string CustomerId { get; set; }
        string SpendRuleId { get; set; }
        string InvoiceId { get; set; }
        Money18 AmountInTokens { get; set; }
        decimal AmountInFiat { get; set; }
        string Currency { get; set; }
        PaymentTransferStatus Status { get; set; }
        DateTime Timestamp { get; set; }
        string LocationCode { get; set; }
        string CustomerAccountNumber { get; set; }
        string CustomerTrxId { get; set; }
        string InstallmentType { get; set; }
        string ReceiptNumber { get; set; }
        string OrgId { get; set; }
        string InstalmentName { get; set; }
    }
}
