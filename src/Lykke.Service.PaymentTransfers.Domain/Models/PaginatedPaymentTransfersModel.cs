using System.Collections.Generic;

namespace Lykke.Service.PaymentTransfers.Domain.Models
{
    public class PaginatedPaymentTransfersModel
    {

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public IEnumerable<IPaymentTransfer> PaymentTransfers { get; set; }
    }
}
