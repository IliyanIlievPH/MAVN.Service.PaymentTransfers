using System.Threading.Tasks;
using MAVN.Service.PaymentTransfers.Domain.Enums;

namespace MAVN.Service.PaymentTransfers.Domain.Models
{
    public interface IPaymentTransfersRepository
    {

        Task AddAsync(PaymentTransferDto paymentTransfer);

        Task SetStatusAsync(string transferId, PaymentTransferStatus status);

        Task<IPaymentTransfer> GetByTransferIdAsync(string transferId);

        Task<long> GetNextSequentialNumberAsync();
    }
}
