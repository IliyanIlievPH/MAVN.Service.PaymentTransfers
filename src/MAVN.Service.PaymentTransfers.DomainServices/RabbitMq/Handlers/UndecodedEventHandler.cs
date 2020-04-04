using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.PaymentTransfers.Contract;
using MAVN.Service.PaymentTransfers.Domain.Common;
using MAVN.Service.PaymentTransfers.Domain.Enums;
using MAVN.Service.PaymentTransfers.Domain.Models;
using MAVN.Service.PaymentTransfers.Domain.RabbitMq.Handlers;
using MAVN.Service.PaymentTransfers.Domain.Services;

namespace MAVN.Service.PaymentTransfers.DomainServices.RabbitMq.Handlers
{
    public class UndecodedEventHandler : IUndecodedEventHandler
    {
        private readonly IBlockchainEventDecoder _eventDecoder;
        private readonly IPaymentsService _paymentsService;
        private readonly IPaymentTransfersRepository _paymentTransfersRepository;
        private readonly IRabbitPublisher<PaymentTransferProcessedEvent> _transferProcessedPublisher;
        private readonly IRabbitPublisher<PaymentTransferTokensReservedEvent> _transferTokensReservedPublisher;
        private readonly ISettingsService _settingsService;
        private readonly ILog _log;

        public UndecodedEventHandler(
            IBlockchainEventDecoder eventDecoder,
            IPaymentsService paymentsService,
            IPaymentTransfersRepository paymentTransfersRepository,
            ILogFactory logFactory,
            IRabbitPublisher<PaymentTransferProcessedEvent> transferProcessedPublisher,
            IRabbitPublisher<PaymentTransferTokensReservedEvent> transferTokensReservedPublisher,
            ISettingsService settingsService)
        {
            _eventDecoder = eventDecoder;
            _paymentsService = paymentsService;
            _paymentTransfersRepository = paymentTransfersRepository;
            _transferProcessedPublisher = transferProcessedPublisher;
            _transferTokensReservedPublisher = transferTokensReservedPublisher;
            _settingsService = settingsService;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(string[] topics, string data, string contractAddress)
        {
            if (!string.Equals(contractAddress,
                _settingsService.GetPaymentTransfersAddress(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                _log.Info("The contract address differs from the expected one. Event handling will be stopped.",
                    context: new {expected = _settingsService.GetPaymentTransfersAddress(), current = contractAddress});

                return;
            }

            var eventType = _eventDecoder.GetEventType(topics[0]);

            switch (eventType)
            {
                case BlockchainEventType.Unknown:
                    return;
                case BlockchainEventType.TransferReceived:
                    await HandleTransferReceived(topics, data);
                    break;
                case BlockchainEventType.TransferAccepted:
                    await HandleTransferAccepted(topics, data);
                    break;
                case BlockchainEventType.TransferRejected:
                    await HandleTransferRejected(topics, data);
                    break;
                default:throw new InvalidOperationException("Unsupported blockchain event type");
            }
        }

        private async Task HandleTransferReceived(string[] topics, string data)
        {
            var transferModel = _eventDecoder.DecodeTransferReceivedEvent(topics, data);

            var paymentTransfer = await _paymentTransfersRepository.GetByTransferIdAsync(transferModel.TransferId);

            if (paymentTransfer == null)
            {
                _log.Error(
                    message:
                    $"Payment transfer with id: {transferModel.TransferId} which was just received was not found in DB",
                    context: transferModel);
                return;
            }

            if (paymentTransfer.Status == PaymentTransferStatus.Pending)
            {
                await _transferTokensReservedPublisher.PublishAsync(new PaymentTransferTokensReservedEvent
                {
                    TransferId = paymentTransfer.TransferId,
                    InvoiceId = paymentTransfer.InvoiceId,
                    Amount = paymentTransfer.AmountInTokens,
                    CustomerId = paymentTransfer.CustomerId,
                    CampaignId = paymentTransfer.SpendRuleId,
                    Timestamp = DateTime.UtcNow,
                    InstalmentName = paymentTransfer.InstalmentName,
                    LocationCode = paymentTransfer.LocationCode,
                });
            }

            var resultError = await _paymentsService.ProcessPaymentTransferAsync(paymentTransfer.TransferId);

            if (resultError != PaymentTransfersErrorCodes.None)
            {
                _log.Error(
                    message:
                    $"Payment transfer with id: {transferModel.TransferId} could not be processed because of error",
                    context: new {transferModel, resultError});
            }
        }

        private async Task HandleTransferAccepted(string[] topics, string data)
        {
            var acceptedTransferModel = _eventDecoder.DecodeTransferAcceptedEvent(topics, data);
            var resultError = await _paymentsService.AcceptPaymentTransferAsync(acceptedTransferModel.TransferId);

            if (resultError != PaymentTransfersErrorCodes.None)
            {
                _log.Error(message:"Unable accept a payment transfer because of error",
                    context: new { Error = resultError, acceptedTransferModel.TransferId });
                return;
            }

            _log.Info("Payment transfer was successfully marked as accepted", context:acceptedTransferModel.TransferId);

            var paymentTransfer = await _paymentTransfersRepository.GetByTransferIdAsync(acceptedTransferModel.TransferId);

            if (paymentTransfer == null)
            {
                throw new InvalidOperationException(
                    $"Payment transfer with id: {acceptedTransferModel.TransferId} which was just accepted was not found in DB");
            }

            await _transferProcessedPublisher.PublishAsync(new PaymentTransferProcessedEvent
            {
                TransferId = paymentTransfer.TransferId,
                Status = ProcessedPaymentTransferStatus.Accepted,
                InvoiceId = paymentTransfer.InvoiceId,
                CampaignId = paymentTransfer.SpendRuleId,
                CustomerId = paymentTransfer.CustomerId,
                Timestamp = DateTime.UtcNow,
                Amount = paymentTransfer.AmountInTokens,
                InstalmentName = paymentTransfer.InstalmentName,
                LocationCode = paymentTransfer.LocationCode,
            });
            _log.Info("Payment transfer processed event published", context:acceptedTransferModel.TransferId);
        }

        private async Task HandleTransferRejected(string[] topics, string data)
        {
            var rejectedTransferModel = _eventDecoder.DecodeTransferRejectedEvent(topics, data);
            var resultError = await _paymentsService.RejectPaymentTransferAsync(rejectedTransferModel.TransferId);

            if (resultError != PaymentTransfersErrorCodes.None)
            {
                _log.Error(message:"Unable reject a payment transfer because of error",
                    context: new { Error = resultError, rejectedTransferModel.TransferId });
                return;
            }

            _log.Info("Payment transfer was successfully marked as rejected", context:rejectedTransferModel.TransferId);

            var paymentTransfer = await _paymentTransfersRepository.GetByTransferIdAsync(rejectedTransferModel.TransferId);

            if (paymentTransfer == null)
            {
                throw new InvalidOperationException(
                    $"Payment transfer with id: {rejectedTransferModel.TransferId} which was just rejected was not found in DB");
            }

            await _transferProcessedPublisher.PublishAsync(new PaymentTransferProcessedEvent
            {
                TransferId = paymentTransfer.TransferId,
                Status = ProcessedPaymentTransferStatus.Rejected,
                InvoiceId = paymentTransfer.InvoiceId,
                CampaignId = paymentTransfer.SpendRuleId,
                CustomerId = paymentTransfer.CustomerId,
                Timestamp = DateTime.UtcNow,
                Amount = paymentTransfer.AmountInTokens,
                InstalmentName = paymentTransfer.InstalmentName,
                LocationCode = paymentTransfer.LocationCode,
            });
            _log.Info("Payment transfer processed event published", context:rejectedTransferModel.TransferId);
        }
    }
}
