using System;
using System.Threading.Tasks;
using Falcon.Numerics;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.PaymentTransfers.Contract;
using MAVN.Service.PaymentTransfers.Domain.Common;
using MAVN.Service.PaymentTransfers.Domain.Enums;
using MAVN.Service.PaymentTransfers.Domain.Models;
using MAVN.Service.PaymentTransfers.Domain.Services;
using MAVN.Service.PaymentTransfers.DomainServices.RabbitMq.Handlers;
using MAVN.Service.PaymentTransfers.MsSqlRepositories;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;
using Moq;
using Xunit;

namespace MAVN.Service.PaymentTransfers.Tests
{
    public class UndecodedEventHandlerTests
    {
        private const string FakeData = "data";
        private const string FakeSourceAddress = "address";
        private const string FakeCustomerId = "customerId";
        private const string FakeCampaignId = "campaignId";
        private const string FakeInvoiceId = "invoiceId";
        private const string FakeTransferId = "trasferId";
        private const string FakeContractAddress = "address";
        private const long FakeAmount = 100;
        private readonly string[] _fakeEventTopics = { "0xEvent" };

        private readonly Mock<IBlockchainEventDecoder> _blockchainEventDecoderMock = new Mock<IBlockchainEventDecoder>();
        private readonly Mock<IPaymentsService> _paymentsServiceMock = new Mock<IPaymentsService>();
        private readonly Mock<IPaymentTransfersRepository> _paymentTransfersRepoMock = new Mock<IPaymentTransfersRepository>();
        private readonly Mock<IPrivateBlockchainFacadeClient> _pbfClientMock = new Mock<IPrivateBlockchainFacadeClient>();
        private readonly Mock<ISettingsService> _settingsServiceMock = new Mock<ISettingsService>();
        private readonly Mock<IRabbitPublisher<PaymentTransferProcessedEvent>> _transferProcessedPublisherMock =
            new Mock<IRabbitPublisher<PaymentTransferProcessedEvent>>();
        private readonly Mock<IRabbitPublisher<PaymentTransferTokensReservedEvent>> _tokensReservedPublisherMock =
            new Mock<IRabbitPublisher<PaymentTransferTokensReservedEvent>>();

        [Fact]
        public async Task HandleUndecodedEvent_EventIsOfTypeWhichIsNotInOurInterest_Return()
        {
            _settingsServiceMock.Setup(x => x.GetPaymentTransfersAddress())
                .Returns(FakeContractAddress);

            _blockchainEventDecoderMock.Setup(x => x.GetEventType(It.IsAny<string>()))
                .Returns(BlockchainEventType.Unknown);

            var sut = CreateSutInstance();

            await sut.HandleAsync(_fakeEventTopics, FakeData, FakeContractAddress);

            _blockchainEventDecoderMock.Verify(
                x => x.DecodeTransferAcceptedEvent(It.IsAny<string[]>(), It.IsAny<string>()), Times.Never);
            _blockchainEventDecoderMock.Verify(
                x => x.DecodeTransferReceivedEvent(It.IsAny<string[]>(), It.IsAny<string>()), Times.Never);
            _blockchainEventDecoderMock.Verify(
                x => x.DecodeTransferRejectedEvent(It.IsAny<string[]>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task HandleUndecodedEvent_EventOfTypeReceived_ErrorFromPbf_EventDataNotStoredInRepo()
        {
            _settingsServiceMock.Setup(x => x.GetPaymentTransfersAddress())
                .Returns(FakeContractAddress);

            _blockchainEventDecoderMock.Setup(x => x.GetEventType(_fakeEventTopics[0]))
                .Returns(BlockchainEventType.TransferReceived);

            _blockchainEventDecoderMock
                .Setup(x => x.DecodeTransferReceivedEvent(_fakeEventTopics, FakeData))
                .Returns(new TransferReceivedModel
                {
                    From = FakeSourceAddress
                });

            _pbfClientMock.Setup(x => x.CustomersApi.GetCustomerIdByWalletAddress(FakeSourceAddress))
                .ReturnsAsync(new CustomerIdByWalletAddressResponse
                {
                    Error = CustomerWalletAddressError.CustomerWalletMissing
                });

            var sut = CreateSutInstance();

            await sut.HandleAsync(_fakeEventTopics, FakeData, FakeContractAddress);

            _paymentTransfersRepoMock.Verify(x => x.AddAsync(It.IsAny<PaymentTransferDto>()), Times.Never);
        }

        [Fact]
        public async Task HandleUndecodedEvent_EventOfTypeReceived_SuccessfullyAdded()
        {
            _settingsServiceMock.Setup(x => x.GetPaymentTransfersAddress())
                .Returns(FakeContractAddress);

            _blockchainEventDecoderMock.Setup(x => x.GetEventType(_fakeEventTopics[0]))
                .Returns(BlockchainEventType.TransferReceived);

            _blockchainEventDecoderMock
                .Setup(x => x.DecodeTransferReceivedEvent(_fakeEventTopics, FakeData))
                .Returns(new TransferReceivedModel
                {
                    From = FakeSourceAddress,
                    TransferId = FakeTransferId,
                    CampaignId = FakeCampaignId,
                    InvoiceId = FakeInvoiceId,
                    Amount = FakeAmount
                });

            _pbfClientMock.Setup(x => x.CustomersApi.GetCustomerIdByWalletAddress(FakeSourceAddress))
                .ReturnsAsync(new CustomerIdByWalletAddressResponse
                {
                    Error = CustomerWalletAddressError.None,
                    CustomerId = FakeCustomerId
                });

            _paymentTransfersRepoMock.Setup(x => x.GetByTransferIdAsync(FakeTransferId))
                .ReturnsAsync(new PaymentTransferEntity
                {
                    TransferId = FakeTransferId,
                    SpendRuleId = FakeCampaignId,
                    InvoiceId = FakeInvoiceId,
                    Status = PaymentTransferStatus.Pending,
                    CustomerId = FakeCustomerId,
                    AmountInTokens = FakeAmount,
                    AmountInFiat = FakeAmount
                });

            var sut = CreateSutInstance();

            await sut.HandleAsync(_fakeEventTopics, FakeData, FakeContractAddress);

            _paymentsServiceMock.Verify(
                x => x.ProcessPaymentTransferAsync(FakeTransferId), Times.Once);

            _tokensReservedPublisherMock.Verify(x =>
                x.PublishAsync(It.Is<PaymentTransferTokensReservedEvent>(obj =>
                    VerifyPaymentTransferTokensReservedEventParameterValues(obj))));
        }

        [Theory]
        [InlineData(PaymentTransfersErrorCodes.InvalidStatus)]
        [InlineData(PaymentTransfersErrorCodes.CustomerWalletDoesNotExist)]
        [InlineData(PaymentTransfersErrorCodes.PaymentTransferAlreadyProcessing)]
        [InlineData(PaymentTransfersErrorCodes.PaymentTransferNotFound)]
        public async Task HandleUndecodedEvent_EventOfTypeAccepted_ErrorOnAccepting_PublisherNotCalled(PaymentTransfersErrorCodes error)
        {
            _settingsServiceMock.Setup(x => x.GetPaymentTransfersAddress())
                .Returns(FakeContractAddress);

            _blockchainEventDecoderMock.Setup(x => x.GetEventType(_fakeEventTopics[0]))
                .Returns(BlockchainEventType.TransferAccepted);

            _blockchainEventDecoderMock
                .Setup(x => x.DecodeTransferAcceptedEvent(_fakeEventTopics, FakeData))
                .Returns(new TransferAcceptedModel
                {
                    TransferId = FakeTransferId
                });

            _paymentsServiceMock.Setup(x => x.AcceptPaymentTransferAsync(FakeTransferId))
                .ReturnsAsync(error);

            var sut = CreateSutInstance();

            await sut.HandleAsync(_fakeEventTopics, FakeData, FakeContractAddress);

            _transferProcessedPublisherMock.Verify(x => x.PublishAsync(It.IsAny<PaymentTransferProcessedEvent>()), Times.Never);
        }

        [Fact]
        public async Task HandleUndecodedEvent_EventOfTypeAccepted_TransferNotFoundInDbAfterItWasAccepted_ExceptionThrown()
        {
            _settingsServiceMock.Setup(x => x.GetPaymentTransfersAddress())
                .Returns(FakeContractAddress);

            _blockchainEventDecoderMock.Setup(x => x.GetEventType(_fakeEventTopics[0]))
                .Returns(BlockchainEventType.TransferAccepted);

            _blockchainEventDecoderMock
                .Setup(x => x.DecodeTransferAcceptedEvent(_fakeEventTopics, FakeData))
                .Returns(new TransferAcceptedModel
                {
                    TransferId = FakeTransferId
                });

            _paymentsServiceMock.Setup(x => x.AcceptPaymentTransferAsync(FakeTransferId))
                .ReturnsAsync(PaymentTransfersErrorCodes.None);

            _paymentTransfersRepoMock.Setup(x => x.GetByTransferIdAsync(FakeTransferId))
                .ReturnsAsync((IPaymentTransfer)null);

            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.HandleAsync(_fakeEventTopics, FakeData, FakeContractAddress));
        }

        [Fact]
        public async Task HandleUndecodedEvent_EventOfTypeAccepted_SuccessfullyProcessed()
        {
            _settingsServiceMock.Setup(x => x.GetPaymentTransfersAddress())
                .Returns(FakeContractAddress);

            _blockchainEventDecoderMock.Setup(x => x.GetEventType(_fakeEventTopics[0]))
                .Returns(BlockchainEventType.TransferAccepted);

            _blockchainEventDecoderMock
                .Setup(x => x.DecodeTransferAcceptedEvent(_fakeEventTopics, FakeData))
                .Returns(new TransferAcceptedModel
                {
                    TransferId = FakeTransferId
                });

            _paymentsServiceMock.Setup(x => x.AcceptPaymentTransferAsync(FakeTransferId))
                .ReturnsAsync(PaymentTransfersErrorCodes.None);

            _paymentTransfersRepoMock.Setup(x => x.GetByTransferIdAsync(FakeTransferId))
                .ReturnsAsync(new PaymentTransferEntity
                {
                    TransferId = FakeTransferId,
                    SpendRuleId = FakeCampaignId,
                    InvoiceId = FakeInvoiceId,
                    Status = PaymentTransferStatus.Accepted,
                    CustomerId = FakeCustomerId,
                    AmountInTokens = FakeAmount,
                    AmountInFiat = FakeAmount
                });

            var sut = CreateSutInstance();

            await sut.HandleAsync(_fakeEventTopics, FakeData, FakeContractAddress);

            _transferProcessedPublisherMock.Verify(x =>
                x.PublishAsync(
                    It.Is<PaymentTransferProcessedEvent>(obj => VerifyAcceptedTransferEventParameterValues(obj))));
        }

        [Theory]
        [InlineData(PaymentTransfersErrorCodes.InvalidStatus)]
        [InlineData(PaymentTransfersErrorCodes.CustomerWalletDoesNotExist)]
        [InlineData(PaymentTransfersErrorCodes.PaymentTransferAlreadyProcessing)]
        [InlineData(PaymentTransfersErrorCodes.PaymentTransferNotFound)]
        public async Task HandleUndecodedEvent_EventOfTypeRejected_ErrorOnAccepting_PublisherNotCalled(PaymentTransfersErrorCodes error)
        {
            _settingsServiceMock.Setup(x => x.GetPaymentTransfersAddress())
                .Returns(FakeContractAddress);

            _blockchainEventDecoderMock.Setup(x => x.GetEventType(_fakeEventTopics[0]))
                .Returns(BlockchainEventType.TransferRejected);

            _blockchainEventDecoderMock
                .Setup(x => x.DecodeTransferRejectedEvent(_fakeEventTopics, FakeData))
                .Returns(new TransferRejectedModel
                {
                    TransferId = FakeTransferId
                });

            _paymentsServiceMock.Setup(x => x.RejectPaymentTransferAsync(FakeTransferId))
                .ReturnsAsync(error);

            var sut = CreateSutInstance();

            await sut.HandleAsync(_fakeEventTopics, FakeData, FakeContractAddress);

            _transferProcessedPublisherMock.Verify(x => x.PublishAsync(It.IsAny<PaymentTransferProcessedEvent>()), Times.Never);
        }

        [Fact]
        public async Task HandleUndecodedEvent_EventOfTypeRejected_TransferNotFoundInDbAfterItWasAccepted_ExceptionThrown()
        {
            _settingsServiceMock.Setup(x => x.GetPaymentTransfersAddress())
                .Returns(FakeContractAddress);

            _blockchainEventDecoderMock.Setup(x => x.GetEventType(_fakeEventTopics[0]))
                .Returns(BlockchainEventType.TransferRejected);

            _blockchainEventDecoderMock
                .Setup(x => x.DecodeTransferRejectedEvent(_fakeEventTopics, FakeData))
                .Returns(new TransferRejectedModel
                {
                    TransferId = FakeTransferId
                });

            _paymentsServiceMock.Setup(x => x.RejectPaymentTransferAsync(FakeTransferId))
                .ReturnsAsync(PaymentTransfersErrorCodes.None);

            _paymentTransfersRepoMock.Setup(x => x.GetByTransferIdAsync(FakeTransferId))
                .ReturnsAsync((IPaymentTransfer)null);

            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.HandleAsync(_fakeEventTopics, FakeData, FakeContractAddress));
        }

        [Fact]
        public async Task HandleUndecodedEvent_EventOfTypeRejected_SuccessfullyProcessed()
        {
            _settingsServiceMock.Setup(x => x.GetPaymentTransfersAddress())
                .Returns(FakeContractAddress);

            _blockchainEventDecoderMock.Setup(x => x.GetEventType(_fakeEventTopics[0]))
                .Returns(BlockchainEventType.TransferRejected);

            _blockchainEventDecoderMock
                .Setup(x => x.DecodeTransferRejectedEvent(_fakeEventTopics, FakeData))
                .Returns(new TransferRejectedModel
                {
                    TransferId = FakeTransferId
                });

            _paymentsServiceMock.Setup(x => x.RejectPaymentTransferAsync(FakeTransferId))
                .ReturnsAsync(PaymentTransfersErrorCodes.None);

            _paymentTransfersRepoMock.Setup(x => x.GetByTransferIdAsync(FakeTransferId))
                .ReturnsAsync(new PaymentTransferEntity()
                {
                    TransferId = FakeTransferId,
                    SpendRuleId = FakeCampaignId,
                    InvoiceId = FakeInvoiceId,
                    Status = PaymentTransferStatus.Rejected,
                    CustomerId = FakeCustomerId,
                    AmountInTokens = FakeAmount,
                    AmountInFiat = FakeAmount
                });

            var sut = CreateSutInstance();

            await sut.HandleAsync(_fakeEventTopics, FakeData, FakeContractAddress);

            _transferProcessedPublisherMock.Verify(x =>
                x.PublishAsync(
                    It.Is<PaymentTransferProcessedEvent>(obj => VerifyRejectedTransferEventParameterValues(obj))));
        }

        [Fact]
        public async Task HandleUndecodedEvent_ContractAddressIsNotPartnerPaymentsOne_EventDecoderNotCalled()
        {
            _settingsServiceMock.Setup(x => x.GetPaymentTransfersAddress())
                .Returns(FakeContractAddress);

            var sut = CreateSutInstance();

            await sut.HandleAsync(_fakeEventTopics, FakeData, "AddressOfNoInterest");

            _blockchainEventDecoderMock.Verify(x => x.GetEventType(It.IsAny<string>()), Times.Never);
        }

        private static bool VerifyReceivedTransferModelParameterValues(PaymentTransferDto model)
        {
            return model.TransferId == FakeTransferId &&
                   model.AmountInFiat == FakeAmount &&
                   model.SpendRuleId == FakeCampaignId &&
                   model.CustomerId == FakeCustomerId &&
                   model.InvoiceId == FakeInvoiceId &&
                   model.Status == PaymentTransferStatus.Pending;
        }

        private static bool VerifyPaymentTransferTokensReservedEventParameterValues(PaymentTransferTokensReservedEvent model)
        {
            return model.TransferId == FakeTransferId &&
                   model.Amount == FakeAmount &&
                   model.CampaignId == FakeCampaignId &&
                   model.CustomerId == FakeCustomerId &&
                   model.InvoiceId == FakeInvoiceId;
        }

        private static bool VerifyAcceptedTransferEventParameterValues(PaymentTransferProcessedEvent model)
        {
            return model.TransferId == FakeTransferId &&
                   model.Amount == FakeAmount &&
                   model.CampaignId == FakeCampaignId &&
                   model.CustomerId == FakeCustomerId &&
                   model.InvoiceId == FakeInvoiceId &&
                   model.Status == ProcessedPaymentTransferStatus.Accepted;
        }

        private static bool VerifyRejectedTransferEventParameterValues(PaymentTransferProcessedEvent model)
        {
            return model.TransferId == FakeTransferId &&
                   model.Amount == FakeAmount &&
                   model.CampaignId == FakeCampaignId &&
                   model.CustomerId == FakeCustomerId &&
                   model.InvoiceId == FakeInvoiceId &&
                   model.Status == ProcessedPaymentTransferStatus.Rejected;
        }

        private UndecodedEventHandler CreateSutInstance()
        {
            return new UndecodedEventHandler(
                _blockchainEventDecoderMock.Object,
                _paymentsServiceMock.Object,
                _paymentTransfersRepoMock.Object,
                EmptyLogFactory.Instance,
                _transferProcessedPublisherMock.Object,
                _tokensReservedPublisherMock.Object,
                _settingsServiceMock.Object);
        }
    }
}
