using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Falcon.Numerics;
using Lykke.Service.PaymentTransfers.Domain.Enums;
using Lykke.Service.PaymentTransfers.Domain.Models;

namespace Lykke.Service.PaymentTransfers.MsSqlRepositories
{
    [Table("payment_transfers")]
    public class PaymentTransferEntity : IPaymentTransfer
    {
        [Key]
        [Column("transfer_id")]
        [Required]
        public string TransferId { get; set; }

        [Column("customer_id")]
        [Required]
        public string CustomerId { get; set; }

        [Column("spend_rule_id")]
        [Required]
        public string SpendRuleId { get; set; }

        [Column("invoice_id")]
        [Required]
        public string InvoiceId { get; set; }

        [Column("amount_in_tokens")]
        [Required]
        public Money18 AmountInTokens { get; set; }

        [Column("amount_in_fiat")]
        [Required]
        public decimal AmountInFiat { get; set; }

        [Column("currency")]
        [Required]
        public string Currency { get; set; }

        [Column("status")]
        [Required]
        public PaymentTransferStatus Status { get; set; }

        [Column("timestamp")]
        [Required]
        public DateTime Timestamp { get; set; }
        
        [Column("sf_location_code")]
        [Required]
        public string LocationCode { get; set; }

        [Column("sf_customer_account_number")]
        [Required]
        public string CustomerAccountNumber { get; set; }

        [Column("sf_customer_trx_id")]
        [Required]
        public string CustomerTrxId { get; set; }

        [Column("sf_installment_type")]
        [Required]
        public string InstallmentType { get; set; }

        [Column("sf_receipt_number")]
        [Required]
        public string ReceiptNumber { get; set; }

        [Column("sf_org_id")]
        [Required]
        public string OrgId { get; set; }

        [Column("sf_instalment_name")]
        [Required]
        public string InstalmentName { get; set; }

        public static PaymentTransferEntity Create(PaymentTransferDto model)
        {
            return new PaymentTransferEntity
            {
                InvoiceId = model.InvoiceId,
                SpendRuleId = model.SpendRuleId,
                CustomerId = model.CustomerId,
                Status = model.Status,
                Timestamp = model.Timestamp,
                TransferId = model.TransferId,
                AmountInTokens = model.AmountInTokens.Value,
                CustomerAccountNumber = model.CustomerAccountNumber,
                CustomerTrxId = model.CustomerTrxId,
                InstallmentType = model.InstallmentType,
                LocationCode = model.LocationCode,
                ReceiptNumber = model.ReceiptNumber,
                AmountInFiat = model.AmountInFiat.Value,
                Currency = model.Currency,
                OrgId = model.OrgId,
                InstalmentName = model.InstalmentName,
            };
        }
    }
}
