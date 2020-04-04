
using Falcon.Numerics;

namespace MAVN.Service.PaymentTransfers.Domain.Models
{
    public class TransferReceivedModel : PaymentTransferModelBase
    {
        public string From { get; set; }
        public Money18 Amount { get; set; }
    }
}
