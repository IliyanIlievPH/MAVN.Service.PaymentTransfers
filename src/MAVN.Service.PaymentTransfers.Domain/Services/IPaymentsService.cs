using System.Threading.Tasks;
using MAVN.Service.PaymentTransfers.Domain.Enums;
using MAVN.Service.PaymentTransfers.Domain.Models;

namespace MAVN.Service.PaymentTransfers.Domain.Services
{
    public interface IPaymentsService
    {
        Task<PaymentTransfersErrorCodes> ProcessPaymentTransferAsync(string transferId);

        Task<PaymentTransfersErrorCodes> AcceptPaymentTransferAsync(string transferId);

        Task<PaymentTransfersErrorCodes> RejectPaymentTransferAsync(string transferId);

        Task<PaymentTransfersErrorCodes> PaymentTransferAsync(PaymentTransferDto paymentTransfer);
    }
}
