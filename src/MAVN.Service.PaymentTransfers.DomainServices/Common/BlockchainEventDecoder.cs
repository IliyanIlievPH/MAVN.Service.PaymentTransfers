using AutoMapper;
using Lykke.PrivateBlockchain.Definitions;
using MAVN.Service.PaymentTransfers.Domain.Common;
using MAVN.Service.PaymentTransfers.Domain.Enums;
using MAVN.Service.PaymentTransfers.Domain.Models;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.Contracts;

namespace MAVN.Service.PaymentTransfers.DomainServices.Common
{
    public class BlockchainEventDecoder : IBlockchainEventDecoder
    {
        private readonly EventTopicDecoder _eventTopicDecoder;
        private readonly string _transferReceivedEventSignature;
        private readonly string _transferAcceptedEventSignature;
        private readonly string _transferRejectedEventSignature;
        private readonly IMapper _mapper;

        public BlockchainEventDecoder(IMapper mapper)
        {
            _eventTopicDecoder = new EventTopicDecoder();
            _transferReceivedEventSignature = $"0x{ABITypedRegistry.GetEvent<TransferReceivedEventDTO>().Sha3Signature}";
            _transferAcceptedEventSignature = $"0x{ABITypedRegistry.GetEvent<TransferAcceptedEventDTO>().Sha3Signature}";
            _transferRejectedEventSignature = $"0x{ABITypedRegistry.GetEvent<TransferRejectedEventDTO>().Sha3Signature}";
            _mapper = mapper;
        }

        public TransferReceivedModel DecodeTransferReceivedEvent(string[] topics, string data)
        {
            var decodedEvent = DecodeEvent<TransferReceivedEventDTO>(topics, data);

            return _mapper.Map<TransferReceivedModel>(decodedEvent);
        }

        public TransferAcceptedModel DecodeTransferAcceptedEvent(string[] topics, string data)
        {
            var decodedEvent = DecodeEvent<TransferAcceptedEventDTO>(topics, data);

            return _mapper.Map<TransferAcceptedModel>(decodedEvent);
        }

        public TransferRejectedModel DecodeTransferRejectedEvent(string[] topics, string data)
        {
            var decodedEvent = DecodeEvent<TransferRejectedEventDTO>(topics, data);

            return _mapper.Map<TransferRejectedModel>(decodedEvent);
        }

        public BlockchainEventType GetEventType(string topic)
        {
            if (topic == _transferReceivedEventSignature)
                return BlockchainEventType.TransferReceived;

            if (topic == _transferAcceptedEventSignature)
                return BlockchainEventType.TransferAccepted;

            if (topic == _transferRejectedEventSignature)
                return BlockchainEventType.TransferRejected;

            return BlockchainEventType.Unknown;
        }

        private T DecodeEvent<T>(string[] topics, string data) where T : class, new()
            => _eventTopicDecoder.DecodeTopics<T>(topics, data);
    }
}
