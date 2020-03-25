using Lykke.Service.PaymentTransfers.Domain.Enums;
using Lykke.Service.PaymentTransfers.Domain.Models;

namespace Lykke.Service.PaymentTransfers.Domain.Common
{
    public interface IBlockchainEventDecoder
    {
        TransferReceivedModel DecodeTransferReceivedEvent(string[] topics, string data);

        TransferAcceptedModel DecodeTransferAcceptedEvent(string[] topics, string data);

        TransferRejectedModel DecodeTransferRejectedEvent(string[] topics, string data);

        BlockchainEventType GetEventType(string topic);
    }
}
