using System;
using System.Globalization;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.PrivateBlockchain.Definitions;
using Lykke.Service.Campaign.Client;
using Lykke.Service.Campaign.Client.Models.Enums;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.EligibilityEngine.Client;
using Lykke.Service.EligibilityEngine.Client.Enums;
using Lykke.Service.EligibilityEngine.Client.Models.ConversionRate.Requests;
using Lykke.Service.MAVNPropertyIntegration.Client;
using Lykke.Service.MAVNPropertyIntegration.Client.Models.Requests;
using Lykke.Service.PartnerManagement.Client.Models;
using Lykke.Service.PaymentTransfers.Domain.Common;
using Lykke.Service.PaymentTransfers.Domain.Enums;
using Lykke.Service.PaymentTransfers.Domain.Models;
using Lykke.Service.PaymentTransfers.Domain.Services;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;
using Lykke.Service.WalletManagement.Client;
using Lykke.Service.WalletManagement.Client.Enums;
using Nethereum.ABI;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding;
using Polly;

namespace Lykke.Service.PaymentTransfers.DomainServices
{
    public class PaymentsService : IPaymentsService
    {
        private const string PaidInvoiceSuccessfullyStatus = "success";

        private readonly ABIEncode _abiEncode;
        private readonly FunctionCallEncoder _functionCallEncoder;
        private readonly IPaymentTransfersRepository _paymentTransfersRepository;
        private readonly ITransactionScopeHandler _transactionScopeHandler;
        private readonly IPrivateBlockchainFacadeClient _pbfClient;
        private readonly IMAVNPropertyIntegrationClient _realEstateIntegrationClient;
        private readonly IEligibilityEngineClient _eligibilityEngineClient;
        private readonly ISettingsService _settingsService;
        private readonly IWalletManagementClient _wmClient;
        private readonly ICustomerProfileClient _cpClient;
        private readonly ICampaignClient _campaignClient;
        private readonly ILog _log;

        public PaymentsService(IPaymentTransfersRepository paymentTransfersRepository,
            ITransactionScopeHandler transactionScopeHandler,
            IPrivateBlockchainFacadeClient pbfClient,
            IMAVNPropertyIntegrationClient realEstateIntegrationClient,
            IEligibilityEngineClient eligibilityEngineClient,
            ISettingsService settingsService,
            IWalletManagementClient wmClient,
            ICustomerProfileClient cpClient,
            ICampaignClient campaignClient,
            ILogFactory logFactory)
        {
            _functionCallEncoder = new FunctionCallEncoder();
            _abiEncode = new ABIEncode();
            _paymentTransfersRepository = paymentTransfersRepository;
            _transactionScopeHandler = transactionScopeHandler;
            _pbfClient = pbfClient;
            _realEstateIntegrationClient = realEstateIntegrationClient;
            _eligibilityEngineClient = eligibilityEngineClient;
            _settingsService = settingsService;
            _wmClient = wmClient;
            _cpClient = cpClient;
            _campaignClient = campaignClient;
            _log = logFactory.CreateLog(this);
        }

        public async Task<PaymentTransfersErrorCodes> PaymentTransferAsync(PaymentTransferDto paymentTransfer)
        {
            #region Validation

            if (!await CheckIfCustomerExists(paymentTransfer.CustomerId))
                return PaymentTransfersErrorCodes.CustomerDoesNotExist;

            if (await CheckIfCustomerWalletIsBlocked(paymentTransfer.CustomerId))
                return PaymentTransfersErrorCodes.CustomerWalletBlocked;

            var isCampaignIdGuid = Guid.TryParse(paymentTransfer.SpendRuleId, out var campaignIdAsGuid);
            if (!isCampaignIdGuid)
                return PaymentTransfersErrorCodes.InvalidSpendRuleId;

            var campaignResult = await _campaignClient.BurnRules.GetByIdAsync(campaignIdAsGuid);

            if (campaignResult.ErrorCode == CampaignServiceErrorCodes.EntityNotFound)
                return PaymentTransfersErrorCodes.SpendRuleNotFound;

            if (campaignResult.Vertical != Vertical.RealEstate)
                return PaymentTransfersErrorCodes.InvalidVerticalInSpendRule;

            //We can have either amount in Tokens or in Fiat
            if (paymentTransfer.AmountInFiat != null && paymentTransfer.AmountInTokens != null)
                return PaymentTransfersErrorCodes.CannotPassBothFiatAndTokensAmount;

            //Fiat or Tokens Amount must be provided
            if (paymentTransfer.AmountInFiat == null && paymentTransfer.AmountInTokens == null)
                return PaymentTransfersErrorCodes.EitherFiatOrTokensAmountShouldBePassed;

            if (paymentTransfer.AmountInTokens != null && paymentTransfer.AmountInTokens <= 0)
                return PaymentTransfersErrorCodes.InvalidTokensAmount;

            if (paymentTransfer.AmountInFiat != null && paymentTransfer.AmountInFiat <= 0)
                return PaymentTransfersErrorCodes.InvalidFiatAmount;

            #endregion

            var conversionRequest = new ConvertAmountBySpendRuleRequest
            {
                CustomerId = Guid.Parse(paymentTransfer.CustomerId),
                Amount = paymentTransfer.AmountInTokens ?? paymentTransfer.AmountInFiat.Value,
                FromCurrency = paymentTransfer.AmountInTokens.HasValue ? _settingsService.GetTokenCurrencyCode() : paymentTransfer.Currency,
                ToCurrency = paymentTransfer.AmountInTokens.HasValue ? paymentTransfer.Currency : _settingsService.GetTokenCurrencyCode(),
                SpendRuleId = campaignIdAsGuid
            };

            var conversionResponse = await _eligibilityEngineClient.ConversionRate.GetAmountBySpendRuleAsync(conversionRequest);

            if (conversionResponse.ErrorCode != EligibilityEngineErrors.None)
            {
                _log.Warning("Invalid amount conversion when trying to add payment transfer", context: conversionRequest);
                return PaymentTransfersErrorCodes.InvalidAmountConversion;
            }

            if (paymentTransfer.AmountInFiat == null)
                paymentTransfer.AmountInFiat = (decimal)conversionResponse.Amount;
            else
                paymentTransfer.AmountInTokens = conversionResponse.Amount;

            var transferId = Guid.NewGuid();
            var transferIdAsString = transferId.ToString();
            var receiptNumber = await GenerateReceiptNumberAsync();

            paymentTransfer.TransferId = transferIdAsString;
            paymentTransfer.ReceiptNumber = receiptNumber;
            paymentTransfer.InvoiceId = receiptNumber;
            paymentTransfer.Timestamp = DateTime.UtcNow;

            var encodedData = EncodePaymentTransferData(paymentTransfer.SpendRuleId, receiptNumber, transferIdAsString);

            var pbfTransferResponse = await _pbfClient.GenericTransfersApi.GenericTransferAsync(new GenericTransferRequestModel
            {
                Amount = paymentTransfer.AmountInTokens.Value,
                AdditionalData = encodedData,
                RecipientAddress = _settingsService.GetPaymentTransfersAddress(),
                SenderCustomerId = paymentTransfer.CustomerId,
                TransferId = transferIdAsString
            });

            if (pbfTransferResponse.Error == TransferError.SenderWalletMissing)
                return PaymentTransfersErrorCodes.CustomerWalletDoesNotExist;

            if (pbfTransferResponse.Error == TransferError.NotEnoughFunds)
                return PaymentTransfersErrorCodes.NotEnoughFunds;

            await _paymentTransfersRepository.AddAsync(paymentTransfer);

            return PaymentTransfersErrorCodes.None;
        }

        public async Task<PaymentTransfersErrorCodes> ProcessPaymentTransferAsync(string transferId)
        {
            if (string.IsNullOrEmpty(transferId))
            {
                _log.Error(message: "TransferId is empty when trying to update payment transfer status");
                throw new ArgumentNullException(nameof(transferId));
            }

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                var paymentTransfer = await _paymentTransfersRepository.GetByTransferIdAsync(transferId);
                if (paymentTransfer == null)
                {
                    _log.Warning("Payment transfer not find by transfer id", context: transferId);
                    return PaymentTransfersErrorCodes.PaymentTransferNotFound;
                }

                if (paymentTransfer.Status == PaymentTransferStatus.Processing)
                    return PaymentTransfersErrorCodes.PaymentTransferAlreadyProcessing;

                if (paymentTransfer.Status != PaymentTransferStatus.Pending)
                {
                    _log.Info("Payment transfer's status cannot be updated because it is not in Pending status");
                    return PaymentTransfersErrorCodes.InvalidStatus;
                }

                var walletAddressResponse = await _pbfClient.CustomersApi.GetWalletAddress(Guid.Parse(paymentTransfer.CustomerId));
                if (walletAddressResponse.Error != CustomerWalletAddressError.None)
                {
                    _log.Warning("Customer transfer does not have a BC wallet when payment transfer is being processed");
                    return PaymentTransfersErrorCodes.CustomerWalletDoesNotExist;
                }

                var payInvoiceResponse = await _realEstateIntegrationClient.Api.PayInvoiceAsync(new InvoicePayRequestModel
                {
                    CurrencyCode = paymentTransfer.Currency,
                    ReceiptNumber = paymentTransfer.ReceiptNumber,
                    CustomerAccountNumber = paymentTransfer.CustomerAccountNumber,
                    CustomerTrxId = paymentTransfer.CustomerTrxId,
                    LocationCode = paymentTransfer.LocationCode,
                    InstallmentType = paymentTransfer.InstallmentType,
                    PaidAmount = paymentTransfer.AmountInFiat,
                    OrgId = paymentTransfer.OrgId,
                });

                _log.Info("Pay invoice response",
                    context: new
                    {
                        payInvoiceResponse.Status,
                        payInvoiceResponse.ErrorCode,
                        payInvoiceResponse.ReceiptNumber,
                        paymentTransfer.TransferId
                    });

                var status = payInvoiceResponse.Status.ToLower() == PaidInvoiceSuccessfullyStatus
                    ? PaymentTransferStatus.Accepted
                    : PaymentTransferStatus.Rejected;

                await CreateApproveOrRejectTransactionInPbf(paymentTransfer.SpendRuleId, paymentTransfer.InvoiceId,
                    paymentTransfer.TransferId, status);
                await _paymentTransfersRepository.SetStatusAsync(paymentTransfer.TransferId, PaymentTransferStatus.Processing);

                _log.Info($"Successfully changed payment transfer's status to Processing", transferId);
                return PaymentTransfersErrorCodes.None;
            });
        }

        public Task<PaymentTransfersErrorCodes> AcceptPaymentTransferAsync(string transferId)
        {
            return AcceptOrRejectAsync(transferId, PaymentTransferStatus.Accepted);
        }

        public Task<PaymentTransfersErrorCodes> RejectPaymentTransferAsync(string transferId)
        {
            return AcceptOrRejectAsync(transferId, PaymentTransferStatus.Rejected);
        }

        private async Task<PaymentTransfersErrorCodes> AcceptOrRejectAsync(string transferId, PaymentTransferStatus status)
        {
            if (string.IsNullOrEmpty(transferId))
            {
                _log.Error(message: "TransferId is empty when trying to update payment transfer status");
                throw new ArgumentNullException(nameof(transferId));
            }

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                var paymentTransfer = await _paymentTransfersRepository.GetByTransferIdAsync(transferId);
                if (paymentTransfer == null)
                {
                    _log.Warning("Payment transfer not find by transfer id", context: transferId);
                    return PaymentTransfersErrorCodes.PaymentTransferNotFound;
                }

                if (paymentTransfer.Status != PaymentTransferStatus.Processing)
                {
                    _log.Info("Payment transfer's status cannot be updated because it is not in Processing status");
                    return PaymentTransfersErrorCodes.InvalidStatus;
                }

                switch (status)
                {
                    case PaymentTransferStatus.Pending:
                    case PaymentTransferStatus.Processing:
                        _log.Warning("Cannot update status to Pending or Processing", context: transferId);
                        return PaymentTransfersErrorCodes.InvalidStatus;
                    case PaymentTransferStatus.Accepted:
                    case PaymentTransferStatus.Rejected:
                        await _paymentTransfersRepository.SetStatusAsync(paymentTransfer.TransferId, status);
                        _log.Info($"Successfully changed payment transfer's status to {status}", context: transferId);
                        return PaymentTransfersErrorCodes.None;
                    default:
                        _log.Error(message: "Trying to update payment transfer's status to unsupported one",
                            context: transferId);
                        return PaymentTransfersErrorCodes.InvalidStatus;
                }
            });
        }

        private async Task CreateApproveOrRejectTransactionInPbf(
            string campaignId,
            string invoiceId,
            string transferId,
            PaymentTransferStatus status)
        {
            string encodedData;
            switch (status)
            {
                case PaymentTransferStatus.Accepted:
                    var acceptFunc = new AcceptTransferFunction
                    {
                        TransferId = transferId,
                        InvoiceId = invoiceId,
                        CampaignId = campaignId
                    };
                    encodedData = EncodeAcceptOrRejectRequestData(acceptFunc);
                    break;
                case PaymentTransferStatus.Rejected:
                    var rejectFunc = new RejectTransferFunction
                    {
                        TransferId = transferId,
                        InvoiceId = invoiceId,
                        CampaignId = campaignId
                    };
                    encodedData = EncodeAcceptOrRejectRequestData(rejectFunc);
                    break;
                default: throw new InvalidOperationException();
            }

            var sourceAddress = _settingsService.GetMasterWalletAddress();
            var targetAddress = _settingsService.GetPaymentTransfersAddress();

            await AddGenericOperationInPbfAsync(encodedData, sourceAddress, targetAddress);
        }

        private string EncodeAcceptOrRejectRequestData<T>(T func)
            where T : class, new()
        {
            var abiFunc = ABITypedRegistry.GetFunctionABI<T>();
            var result = _functionCallEncoder.EncodeRequest(func, abiFunc.Sha3Signature);

            return result;
        }

        private Task AddGenericOperationInPbfAsync(string data, string sourceAddress, string targetAddress)
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    _settingsService.GetMaxAcceptOrRejectTransactionInPbfRetryCount(),
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (ex, _) => _log.Error("Creating operation for accept/reject payment transfer in pbf with retry", ex))
                .ExecuteAsync(() => _pbfClient.OperationsApi.AddGenericOperationAsync(new GenericOperationRequest
                {
                    Data = data,
                    SourceAddress = sourceAddress,
                    TargetAddress = targetAddress
                }));
        }

        private async Task<bool> CheckIfCustomerExists(string customerId)
        {
            var customer = await _cpClient.CustomerProfiles.GetByCustomerIdAsync(customerId);

            return customer?.Profile != null;
        }

        private async Task<bool> CheckIfCustomerWalletIsBlocked(string customerId)
        {
            var isBlocked = await _wmClient.Api.GetCustomerWalletBlockStateAsync(customerId);

            return isBlocked.Status == CustomerWalletActivityStatus.Blocked;
        }

        private string EncodePaymentTransferData(string campaignId, string invoiceId, string transferId)
        {
            var parameters = new[]
            {
                new ABIValue(new StringType(), campaignId),
                new ABIValue(new StringType(), invoiceId),
                new ABIValue(new StringType(), transferId),
            };

            return _abiEncode.GetABIEncoded(parameters).ToHex(true);
        }

        private async Task<string> GenerateReceiptNumberAsync()
        {
            var sequentialNumber = await _paymentTransfersRepository.GetNextSequentialNumberAsync();

            var date = DateTime.UtcNow.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

            return $"Token/{date}/{sequentialNumber}";
        }
    }
}
