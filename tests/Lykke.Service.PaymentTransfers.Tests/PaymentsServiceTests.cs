using System;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.Service.Campaign.Client;
using Lykke.Service.Campaign.Client.Models.BurnRule.Responses;
using Lykke.Service.Campaign.Client.Models.Enums;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.CustomerProfile.Client.Models.Responses;
using Lykke.Service.EligibilityEngine.Client;
using Lykke.Service.EligibilityEngine.Client.Enums;
using Lykke.Service.EligibilityEngine.Client.Models.ConversionRate.Requests;
using Lykke.Service.EligibilityEngine.Client.Models.ConversionRate.Responses;
using Lykke.Service.MAVNPropertyIntegration.Client;
using Lykke.Service.MAVNPropertyIntegration.Client.Models.Requests;
using Lykke.Service.MAVNPropertyIntegration.Client.Models.Responses;
using Lykke.Service.PartnerManagement.Client.Models;
using Lykke.Service.PaymentTransfers.Domain.Common;
using Lykke.Service.PaymentTransfers.Domain.Enums;
using Lykke.Service.PaymentTransfers.Domain.Models;
using Lykke.Service.PaymentTransfers.Domain.Services;
using Lykke.Service.PaymentTransfers.DomainServices;
using Lykke.Service.PaymentTransfers.DomainServices.Common;
using Lykke.Service.PaymentTransfers.MsSqlRepositories;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;
using Lykke.Service.WalletManagement.Client;
using Lykke.Service.WalletManagement.Client.Enums;
using Lykke.Service.WalletManagement.Client.Models.Responses;
using Moq;
using Xunit;

namespace Lykke.Service.PaymentTransfers.Tests
{
    public class PaymentsServiceTests
    {
        private const string FakeTransferId = "fakeId";
        private const string FakeCustomerId = "13c49920-79bd-4312-87a8-c15eb8ce20f2";
        private const string FakeCampaignId = "13c49920-79bd-4312-87a8-c15eb8ce20f2";
        private const string FakeInvoiceId = "invoiceId";
        private const string FakeTargetAddress = "targetAddress";
        private const string FakeCustomerWalletAddress = "customerAddress";
        private const string FakeCurrencyAsset = "AED";
        private const string PayInvoiceApproveResponse = "Success";
        private const string PayInvoiceRejectedResponse = "Rejected";
        private const string FakeAdditionalData = "0xData";
        private const string FakeAddress = "address";
        private const string InvalidCampaignId = "asdf";
        private const long ValidAmount = 100;

        private readonly Mock<IPaymentTransfersRepository> _paymentTransfersRepositoryMock = new Mock<IPaymentTransfersRepository>();
        private readonly Mock<IPrivateBlockchainFacadeClient> _pbfClientMock = new Mock<IPrivateBlockchainFacadeClient>();
        private readonly Mock<IMAVNPropertyIntegrationClient> _realEstateIntegrationClientMock = new Mock<IMAVNPropertyIntegrationClient>();
        private readonly Mock<ISettingsService> _settingsServiceMock = new Mock<ISettingsService>();
        private readonly Mock<IEligibilityEngineClient> _eligibilityEngineClientMock = new Mock<IEligibilityEngineClient>();
        private readonly Mock<IWalletManagementClient> _wmClient = new Mock<IWalletManagementClient>();
        private readonly Mock<ICustomerProfileClient> _cpClient = new Mock<ICustomerProfileClient>();
        private readonly Mock<ICampaignClient> _campaignClient = new Mock<ICampaignClient>();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ProcessPaymentTransfer_EmptyTransferId_ArgumentNullExceptionIsThrown(string transferId)
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.ProcessPaymentTransferAsync(transferId));
        }

        [Fact]
        public async Task ProcessPaymentTransfer_TransferNotFound_ErrorCodeReturned()
        {
            _paymentTransfersRepositoryMock.Setup(x => x.GetByTransferIdAsync(It.IsAny<string>()))
                .ReturnsAsync((IPaymentTransfer)null);

            var sut = CreateSutInstance();

            var result = await sut.ProcessPaymentTransferAsync(FakeTransferId);

            Assert.Equal(PaymentTransfersErrorCodes.PaymentTransferNotFound, result);
        }

        [Fact]
        public async Task ProcessPaymentTransfer_TransferAlreadyInProcessing_ErrorCodeReturned()
        {
            _paymentTransfersRepositoryMock.Setup(x => x.GetByTransferIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaymentTransferEntity
                {
                    Status = PaymentTransferStatus.Processing
                });

            var sut = CreateSutInstance();

            var result = await sut.ProcessPaymentTransferAsync(FakeTransferId);

            Assert.Equal(PaymentTransfersErrorCodes.PaymentTransferAlreadyProcessing, result);
        }

        [Theory]
        [InlineData(PaymentTransferStatus.Accepted)]
        [InlineData(PaymentTransferStatus.Rejected)]
        public async Task ProcessPaymentTransfer_TransferNotInPendingStatus_ErrorCodeReturned(PaymentTransferStatus status)
        {
            _paymentTransfersRepositoryMock.Setup(x => x.GetByTransferIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaymentTransferEntity
                {
                    Status = status
                });

            var sut = CreateSutInstance();

            var result = await sut.ProcessPaymentTransferAsync(FakeTransferId);

            Assert.Equal(PaymentTransfersErrorCodes.InvalidStatus, result);
        }

        [Fact]
        public async Task ProcessPaymentTransfer_CustomerWalletNotFound_ErrorCodeReturned()
        {
            _paymentTransfersRepositoryMock.Setup(x => x.GetByTransferIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaymentTransferEntity
                {
                    Status = PaymentTransferStatus.Pending,
                    CustomerId = FakeCustomerId
                });

            _pbfClientMock.Setup(x => x.CustomersApi.GetWalletAddress(It.IsAny<Guid>()))
                .ReturnsAsync(new CustomerWalletAddressResponseModel
                {
                    Error = CustomerWalletAddressError.CustomerWalletMissing
                });

            var sut = CreateSutInstance();

            var result = await sut.ProcessPaymentTransferAsync(FakeTransferId);

            Assert.Equal(PaymentTransfersErrorCodes.CustomerWalletDoesNotExist, result);
        }

        [Fact]
        public async Task ProcessPaymentTransferToRejected_EverythingValid_RepositoryCalledToChangeStatus()
        {
            _paymentTransfersRepositoryMock.Setup(x => x.GetByTransferIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaymentTransferEntity
                {
                    Status = PaymentTransferStatus.Pending,
                    CustomerId = FakeCustomerId,
                    TransferId = FakeTransferId,
                    SpendRuleId = FakeCampaignId,
                    InvoiceId = FakeInvoiceId
                });

            _pbfClientMock.Setup(x => x.CustomersApi.GetWalletAddress(It.IsAny<Guid>()))
                .ReturnsAsync(new CustomerWalletAddressResponseModel
                {
                    Error = CustomerWalletAddressError.None,
                    WalletAddress = FakeCustomerWalletAddress
                });

            _settingsServiceMock.Setup(x => x.GetPaymentTransfersAddress())
                .Returns(FakeTargetAddress);

            _realEstateIntegrationClientMock.Setup(x => x.Api.PayInvoiceAsync(It.IsAny<InvoicePayRequestModel>()))
                .ReturnsAsync(new InvoicePayResponseModel { Status = PayInvoiceRejectedResponse });

            _pbfClientMock.Setup(x => x.OperationsApi.AddGenericOperationAsync(It.IsAny<GenericOperationRequest>()))
                .ReturnsAsync(new GenericOperationResponse())
                .Verifiable();

            _paymentTransfersRepositoryMock.Setup(x => x.SetStatusAsync(FakeTransferId, PaymentTransferStatus.Processing))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var sut = CreateSutInstance();

            await sut.ProcessPaymentTransferAsync(FakeTransferId);

            _pbfClientMock.Verify();
            _paymentTransfersRepositoryMock.Verify();
        }


        [Fact]
        public async Task ProcessPaymentTransferToApproved_EverythingSuccessful_RepositoryCalledToChangeStatus()
        {
            _paymentTransfersRepositoryMock.Setup(x => x.GetByTransferIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaymentTransferEntity
                {
                    Status = PaymentTransferStatus.Pending,
                    CustomerId = FakeCustomerId,
                    TransferId = FakeTransferId,
                    SpendRuleId = FakeCampaignId,
                    InvoiceId = FakeInvoiceId
                });

            _pbfClientMock.Setup(x => x.CustomersApi.GetWalletAddress(It.IsAny<Guid>()))
                .ReturnsAsync(new CustomerWalletAddressResponseModel
                {
                    Error = CustomerWalletAddressError.None,
                    WalletAddress = FakeCustomerWalletAddress
                });

            _eligibilityEngineClientMock.Setup(x => x.ConversionRate.GetAmountBySpendRuleAsync(It.IsAny<ConvertAmountBySpendRuleRequest>()))
                .ReturnsAsync(new ConvertAmountBySpendRuleResponse
                {
                    Amount = ValidAmount,
                    CurrencyCode = FakeCurrencyAsset
                });

            _pbfClientMock.Setup(x => x.OperationsApi.AddGenericOperationAsync(It.IsAny<GenericOperationRequest>()))
                .ReturnsAsync(new GenericOperationResponse())
                .Verifiable();

            _paymentTransfersRepositoryMock.Setup(x => x.SetStatusAsync(FakeTransferId, PaymentTransferStatus.Processing))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _realEstateIntegrationClientMock.Setup(x => x.Api.PayInvoiceAsync(It.IsAny<InvoicePayRequestModel>()))
                .ReturnsAsync(new InvoicePayResponseModel { Status = PayInvoiceApproveResponse });

            _settingsServiceMock.Setup(x => x.GetMaxAcceptOrRejectTransactionInPbfRetryCount())
                .Returns(2);

            _settingsServiceMock.Setup(x => x.GetPaymentTransfersAddress())
                .Returns(FakeTargetAddress);

            var sut = CreateSutInstance();

            var result = await sut.ProcessPaymentTransferAsync(FakeTransferId);

            Assert.Equal(PaymentTransfersErrorCodes.None, result);
            _pbfClientMock.Verify();
            _paymentTransfersRepositoryMock.Verify();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task AcceptPaymentTransfer_InvalidTransferId_ArgumentNullExceptionIsThrown(string transferId)
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.AcceptPaymentTransferAsync(transferId));
        }

        [Fact]
        public async Task AcceptPaymentTransfer_TransferNotFound_ErrorCodeReturned()
        {
            _paymentTransfersRepositoryMock.Setup(x => x.GetByTransferIdAsync(It.IsAny<string>()))
                .ReturnsAsync((IPaymentTransfer)null);

            var sut = CreateSutInstance();

            var result = await sut.AcceptPaymentTransferAsync(FakeTransferId);

            Assert.Equal(PaymentTransfersErrorCodes.PaymentTransferNotFound, result);
        }


        [Theory]
        [InlineData(PaymentTransferStatus.Accepted)]
        [InlineData(PaymentTransferStatus.Pending)]
        [InlineData(PaymentTransferStatus.Rejected)]
        public async Task ProcessPaymentTransfer_TransferNotInProcessing_ErrorCodeReturned(PaymentTransferStatus status)
        {
            _paymentTransfersRepositoryMock.Setup(x => x.GetByTransferIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaymentTransferEntity
                {
                    Status = status
                });

            var sut = CreateSutInstance();

            var result = await sut.AcceptPaymentTransferAsync(FakeTransferId);

            Assert.Equal(PaymentTransfersErrorCodes.InvalidStatus, result);
        }

        [Fact]
        public async Task AcceptPaymentTransfer_EverythingValid_RepositoryCalledToChangeStatus()
        {
            _paymentTransfersRepositoryMock.Setup(x => x.GetByTransferIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaymentTransferEntity
                {
                    Status = PaymentTransferStatus.Processing,
                    CustomerId = FakeCustomerId,
                    TransferId = FakeTransferId,
                    SpendRuleId = FakeCampaignId,
                    InvoiceId = FakeInvoiceId
                });

            _pbfClientMock.Setup(x => x.CustomersApi.GetWalletAddress(It.IsAny<Guid>()))
                .ReturnsAsync(new CustomerWalletAddressResponseModel
                {
                    Error = CustomerWalletAddressError.None,
                    WalletAddress = FakeCustomerWalletAddress
                });
            _settingsServiceMock.Setup(x => x.GetPaymentTransfersAddress())
                .Returns(FakeTargetAddress);

            _paymentTransfersRepositoryMock.Setup(x => x.SetStatusAsync(FakeTransferId, PaymentTransferStatus.Accepted))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var sut = CreateSutInstance();

            var result = await sut.AcceptPaymentTransferAsync(FakeTransferId);

            _paymentTransfersRepositoryMock.Verify();
        }

        [Fact]
        public async Task RejectPaymentTransfer_EverythingValid_RepositoryCalledToChangeStatus()
        {
            _paymentTransfersRepositoryMock.Setup(x => x.GetByTransferIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new PaymentTransferEntity
                {
                    Status = PaymentTransferStatus.Processing,
                    CustomerId = FakeCustomerId,
                    TransferId = FakeTransferId,
                    SpendRuleId = FakeCampaignId,
                    InvoiceId = FakeInvoiceId
                });

            _pbfClientMock.Setup(x => x.CustomersApi.GetWalletAddress(It.IsAny<Guid>()))
                .ReturnsAsync(new CustomerWalletAddressResponseModel
                {
                    Error = CustomerWalletAddressError.None,
                    WalletAddress = FakeCustomerWalletAddress
                });
            _settingsServiceMock.Setup(x => x.GetPaymentTransfersAddress())
                .Returns(FakeTargetAddress);

            _paymentTransfersRepositoryMock.Setup(x => x.SetStatusAsync(FakeTransferId, PaymentTransferStatus.Rejected))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var sut = CreateSutInstance();

            var result = await sut.RejectPaymentTransferAsync(FakeTransferId);

            _paymentTransfersRepositoryMock.Verify();
        }

        [Fact]
        public async Task CustomerTriesToMakePaymentTransfer_CustomerDoesNotExists_ErrorReturned()
        {
            _cpClient.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = null
                });

            var sut = CreateSutInstance();

            var result = await sut.PaymentTransferAsync(new PaymentTransferDto { CustomerId = FakeCustomerId });

            Assert.Equal(PaymentTransfersErrorCodes.CustomerDoesNotExist, result);
        }

        [Fact]
        public async Task CustomerTriesToMakePaymentTransfer_WalletIsBlocked_ErrorReturned()
        {
            _cpClient.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile()
                });

            _wmClient
                .Setup(x => x.Api.GetCustomerWalletBlockStateAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse { Status = CustomerWalletActivityStatus.Blocked });

            var sut = CreateSutInstance();

            var result = await sut.PaymentTransferAsync(new PaymentTransferDto { CustomerId = FakeCustomerId });

            Assert.Equal(PaymentTransfersErrorCodes.CustomerWalletBlocked, result);
        }

        [Fact]
        public async Task CustomerTriesToMakePaymentTransfer_InvalidCampaignId_ErrorReturned()
        {
            _cpClient.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile()
                });

            _wmClient
                .Setup(x => x.Api.GetCustomerWalletBlockStateAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse { Status = CustomerWalletActivityStatus.Active });

            var sut = CreateSutInstance();

            var result = await sut.PaymentTransferAsync(new PaymentTransferDto { CustomerId = FakeCustomerId, SpendRuleId = InvalidCampaignId });

            Assert.Equal(PaymentTransfersErrorCodes.InvalidSpendRuleId, result);
        }

        [Fact]
        public async Task CustomerTriesToMakePaymentTransfer_CampaignDoesNotExist_ErrorReturned()
        {
            _cpClient.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile()
                });

            _wmClient
                .Setup(x => x.Api.GetCustomerWalletBlockStateAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse { Status = CustomerWalletActivityStatus.Active });

            _campaignClient.Setup(x => x.BurnRules.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new BurnRuleResponse { ErrorCode = CampaignServiceErrorCodes.EntityNotFound });


            var sut = CreateSutInstance();

            var result = await sut.PaymentTransferAsync(new PaymentTransferDto { CustomerId = FakeCustomerId, SpendRuleId = FakeCampaignId });

            Assert.Equal(PaymentTransfersErrorCodes.SpendRuleNotFound, result);
        }

        [Fact]
        public async Task CustomerTriesToMakePaymentTransfer_InvalidVerticalInSpendRule_ErrorReturned()
        {
            _cpClient.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile()
                });

            _wmClient
                .Setup(x => x.Api.GetCustomerWalletBlockStateAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse { Status = CustomerWalletActivityStatus.Active });

            _campaignClient.Setup(x => x.BurnRules.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new BurnRuleResponse { ErrorCode = CampaignServiceErrorCodes.None, Vertical = Vertical.Hospitality });

            var requestDto = new PaymentTransferDto
            {
                CustomerId = FakeCustomerId,
                SpendRuleId = FakeCampaignId,
                AmountInTokens = ValidAmount,
            };

            var sut = CreateSutInstance();

            var result = await sut.PaymentTransferAsync(requestDto);

            Assert.Equal(PaymentTransfersErrorCodes.InvalidVerticalInSpendRule, result);
        }

        [Fact]
        public async Task CustomerTriesToMakePaymentTransfer_BothAmountInFiatAndInTokensPassed_ErrorReturned()
        {
            _cpClient.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile()
                });

            _wmClient
                .Setup(x => x.Api.GetCustomerWalletBlockStateAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse { Status = CustomerWalletActivityStatus.Active });

            _campaignClient.Setup(x => x.BurnRules.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new BurnRuleResponse { ErrorCode = CampaignServiceErrorCodes.None, Vertical = Vertical.RealEstate });

            var requestDto = new PaymentTransferDto
            {
                CustomerId = FakeCustomerId,
                SpendRuleId = FakeCampaignId,
                AmountInTokens = ValidAmount,
                AmountInFiat = ValidAmount
            };

            var sut = CreateSutInstance();

            var result = await sut.PaymentTransferAsync(requestDto);

            Assert.Equal(PaymentTransfersErrorCodes.CannotPassBothFiatAndTokensAmount, result);
        }

        [Fact]
        public async Task CustomerTriesToMakePaymentTransfer_NeitherAmountInTokensNorInFiatIsPassed_ErrorReturned()
        {
            _cpClient.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile()
                });

            _wmClient
                .Setup(x => x.Api.GetCustomerWalletBlockStateAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse { Status = CustomerWalletActivityStatus.Active });

            _campaignClient.Setup(x => x.BurnRules.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new BurnRuleResponse { ErrorCode = CampaignServiceErrorCodes.None, Vertical = Vertical.RealEstate });

            var requestDto = new PaymentTransferDto
            {
                CustomerId = FakeCustomerId,
                SpendRuleId = FakeCampaignId,
                AmountInTokens = null,
                AmountInFiat = null
            };

            var sut = CreateSutInstance();

            var result = await sut.PaymentTransferAsync(requestDto);

            Assert.Equal(PaymentTransfersErrorCodes.EitherFiatOrTokensAmountShouldBePassed, result);
        }

        [Fact]
        public async Task CustomerTriesToMakePaymentTransfer_InvalidAmountInTokens_ErrorReturned()
        {
            _cpClient.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile()
                });

            _wmClient
                .Setup(x => x.Api.GetCustomerWalletBlockStateAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse { Status = CustomerWalletActivityStatus.Active });

            _campaignClient.Setup(x => x.BurnRules.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new BurnRuleResponse { ErrorCode = CampaignServiceErrorCodes.None, Vertical = Vertical.RealEstate });

            var requestDto = new PaymentTransferDto
            {
                CustomerId = FakeCustomerId,
                SpendRuleId = FakeCampaignId,
                AmountInTokens = 0
            };

            var sut = CreateSutInstance();

            var result = await sut.PaymentTransferAsync(requestDto);

            Assert.Equal(PaymentTransfersErrorCodes.InvalidTokensAmount, result);
        }

        [Fact]
        public async Task CustomerTriesToMakePaymentTransfer_InvalidAmountInFiat_ErrorReturned()
        {
            _cpClient.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile()
                });

            _wmClient
                .Setup(x => x.Api.GetCustomerWalletBlockStateAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse { Status = CustomerWalletActivityStatus.Active });

            _campaignClient.Setup(x => x.BurnRules.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new BurnRuleResponse { ErrorCode = CampaignServiceErrorCodes.None, Vertical = Vertical.RealEstate });

            var requestDto = new PaymentTransferDto
            {
                CustomerId = FakeCustomerId,
                SpendRuleId = FakeCampaignId,
                AmountInFiat = 0
            };

            var sut = CreateSutInstance();

            var result = await sut.PaymentTransferAsync(requestDto);

            Assert.Equal(PaymentTransfersErrorCodes.InvalidFiatAmount, result);
        }

        [Fact]
        public async Task CustomerTriesToMakePaymentTransfer_ErrorFromEligibilityEngine_ErrorReturned()
        {
            _cpClient.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile()
                });

            _wmClient
                .Setup(x => x.Api.GetCustomerWalletBlockStateAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse { Status = CustomerWalletActivityStatus.Active });

            _campaignClient.Setup(x => x.BurnRules.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new BurnRuleResponse { ErrorCode = CampaignServiceErrorCodes.None, Vertical = Vertical.RealEstate });

            _eligibilityEngineClientMock.Setup(x =>
                    x.ConversionRate.GetAmountBySpendRuleAsync(It.IsAny<ConvertAmountBySpendRuleRequest>()))
                .ReturnsAsync(new ConvertAmountBySpendRuleResponse
                {
                    ErrorCode = EligibilityEngineErrors.ConversionRateNotFound
                });

            var requestDto = new PaymentTransferDto
            {
                CustomerId = FakeCustomerId,
                SpendRuleId = FakeCampaignId,
                AmountInFiat = ValidAmount
            };

            var sut = CreateSutInstance();

            var result = await sut.PaymentTransferAsync(requestDto);

            Assert.Equal(PaymentTransfersErrorCodes.InvalidAmountConversion, result);
        }

        [Theory]
        [InlineData(TransferError.SenderWalletMissing, PaymentTransfersErrorCodes.CustomerWalletDoesNotExist)]
        [InlineData(TransferError.NotEnoughFunds, PaymentTransfersErrorCodes.NotEnoughFunds)]
        public async Task CustomerTriesToMakePaymentTransfer_ErrorFromPbf_ErrorReturned
            (TransferError pbfError, PaymentTransfersErrorCodes expectedError)
        {
            _cpClient.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile()
                });

            _wmClient
                .Setup(x => x.Api.GetCustomerWalletBlockStateAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse { Status = CustomerWalletActivityStatus.Active });

            _campaignClient.Setup(x => x.BurnRules.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new BurnRuleResponse { ErrorCode = CampaignServiceErrorCodes.None, Vertical = Vertical.RealEstate });

            _eligibilityEngineClientMock.Setup(x =>
                    x.ConversionRate.GetAmountBySpendRuleAsync(It.IsAny<ConvertAmountBySpendRuleRequest>()))
                .ReturnsAsync(new ConvertAmountBySpendRuleResponse
                {
                    ErrorCode = EligibilityEngineErrors.None,
                    Amount = ValidAmount
                });

            _pbfClientMock.Setup(x =>
                    x.GenericTransfersApi.GenericTransferAsync(It.IsAny<GenericTransferRequestModel>()))
                .ReturnsAsync(new TransferResponseModel { Error = pbfError });

            var requestDto = new PaymentTransferDto
            {
                CustomerId = FakeCustomerId,
                SpendRuleId = FakeCampaignId,
                AmountInFiat = ValidAmount
            };

            var sut = CreateSutInstance();

            var result = await sut.PaymentTransferAsync(requestDto);

            Assert.Equal(expectedError, result);
        }

        [Fact]
        public async Task CustomerTriesToMakePaymentTransfer_SuccessfullyCreated()
        {
            _cpClient.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile()
                });

            _wmClient
                .Setup(x => x.Api.GetCustomerWalletBlockStateAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse { Status = CustomerWalletActivityStatus.Active });

            _campaignClient.Setup(x => x.BurnRules.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new BurnRuleResponse { ErrorCode = CampaignServiceErrorCodes.None, Vertical = Vertical.RealEstate });

            _eligibilityEngineClientMock.Setup(x =>
                    x.ConversionRate.GetAmountBySpendRuleAsync(It.IsAny<ConvertAmountBySpendRuleRequest>()))
                .ReturnsAsync(new ConvertAmountBySpendRuleResponse
                {
                    ErrorCode = EligibilityEngineErrors.None,
                    Amount = 2*ValidAmount
                });

            _pbfClientMock.Setup(x =>
                    x.GenericTransfersApi.GenericTransferAsync(It.IsAny<GenericTransferRequestModel>()))
                .ReturnsAsync(new TransferResponseModel { Error = TransferError.None });

            var requestDto = new PaymentTransferDto
            {
                CustomerId = FakeCustomerId,
                SpendRuleId = FakeCampaignId,
                AmountInFiat = ValidAmount
            };

            var amountInTokens = 2 * ValidAmount;

            var sut = CreateSutInstance();

            var result = await sut.PaymentTransferAsync(requestDto);

            Assert.Equal(PaymentTransfersErrorCodes.None, result);
            _paymentTransfersRepositoryMock.Verify(x => x.AddAsync(It.Is<PaymentTransferDto>(p =>
                p.AmountInTokens == amountInTokens && p.AmountInFiat == ValidAmount && p.ReceiptNumber.Length <= 30)));
            _pbfClientMock.Verify(x => x.GenericTransfersApi.GenericTransferAsync(It.Is<GenericTransferRequestModel>(t => t.Amount == amountInTokens)), Times.Once);
        }


        private IPaymentsService CreateSutInstance()
        {
            return new PaymentsService(
                _paymentTransfersRepositoryMock.Object,
                new TransactionScopeHandler(EmptyLogFactory.Instance),
                _pbfClientMock.Object,
                _realEstateIntegrationClientMock.Object,
                _eligibilityEngineClientMock.Object,
                _settingsServiceMock.Object,
                _wmClient.Object,
                _cpClient.Object,
                _campaignClient.Object,
                EmptyLogFactory.Instance);
        }
    }
}
