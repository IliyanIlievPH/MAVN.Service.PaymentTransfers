namespace MAVN.Service.PaymentTransfers.Domain.Models
{
    public abstract class PaymentTransferModelBase
    {
        public string CampaignId { get; set; }
        public string InvoiceId { get; set; }
        public string TransferId { get; set; }
    }
}
