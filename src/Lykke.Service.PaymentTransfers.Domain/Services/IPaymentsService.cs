using System.Threading.Tasks;
using Lykke.Service.PaymentTransfers.Domain.Enums;
using Lykke.Service.PaymentTransfers.Domain.Models;

namespace Lykke.Service.PaymentTransfers.Domain.Services
{
    public interface IPaymentsService
    {
        Task<PaymentTransfersErrorCodes> ProcessPaymentTransferAsync(string transferId);

        Task<PaymentTransfersErrorCodes> AcceptPaymentTransferAsync(string transferId);

        Task<PaymentTransfersErrorCodes> RejectPaymentTransferAsync(string transferId);

        Task<PaymentTransfersErrorCodes> PaymentTransferAsync(PaymentTransferDto paymentTransfer);
    }
}
