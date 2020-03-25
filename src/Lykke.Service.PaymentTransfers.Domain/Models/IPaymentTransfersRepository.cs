using System.Threading.Tasks;
using Lykke.Service.PaymentTransfers.Domain.Enums;

namespace Lykke.Service.PaymentTransfers.Domain.Models
{
    public interface IPaymentTransfersRepository
    {

        Task AddAsync(PaymentTransferDto paymentTransfer);

        Task SetStatusAsync(string transferId, PaymentTransferStatus status);

        Task<IPaymentTransfer> GetByTransferIdAsync(string transferId);

        Task<long> GetNextSequentialNumberAsync();
    }
}
