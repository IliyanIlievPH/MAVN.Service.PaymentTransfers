using Autofac;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.Campaign.Client;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.EligibilityEngine.Client;
using Lykke.Service.MAVNPropertyIntegration.Client;
using MAVN.Service.PaymentTransfers.Domain.Common;
using MAVN.Service.PaymentTransfers.Domain.RabbitMq.Handlers;
using MAVN.Service.PaymentTransfers.Domain.Services;
using MAVN.Service.PaymentTransfers.DomainServices;
using MAVN.Service.PaymentTransfers.DomainServices.Common;
using MAVN.Service.PaymentTransfers.DomainServices.RabbitMq.Handlers;
using MAVN.Service.PaymentTransfers.Services;
using MAVN.Service.PaymentTransfers.Settings;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.WalletManagement.Client;
using Lykke.SettingsReader;

namespace MAVN.Service.PaymentTransfers.Modules
{
    [UsedImplicitly]
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SettingsService>()
                .As<ISettingsService>()
                .WithParameter("paymentTransfersAddress",
                    _appSettings.CurrentValue.PaymentTransfersService.PaymentTransfersAddress)
                .WithParameter("masterWalletAddress",
                    _appSettings.CurrentValue.PaymentTransfersService.MasterWalletAddress)
                .WithParameter("defaultCurrency",
                    _appSettings.CurrentValue.PaymentTransfersService.DefaultCurrency)
                .WithParameter("tokenCurrencyCode",
                    _appSettings.CurrentValue.PaymentTransfersService.TokenCurrencyCode)
                .WithParameter("maxAcceptOrRejectTransactionInPbfRetryCount",
                    _appSettings.CurrentValue.PaymentTransfersService.MaxAcceptOrRejectTransactionInPbfRetryCount)
                .SingleInstance();

            builder.RegisterType<PaymentsService>()
                .As<IPaymentsService>()
                .SingleInstance();

            builder.RegisterType<TransactionScopeHandler>()
                .As<ITransactionScopeHandler>()
                .InstancePerLifetimeScope();

            builder.RegisterPrivateBlockchainFacadeClient(_appSettings.CurrentValue.PrivateBlockchainFacadeService, null);

            builder.RegisterMAVNPropertyIntegrationClient(_appSettings.CurrentValue.RealEstateIntegrationService, null);

            builder.RegisterEligibilityEngineClient(_appSettings.CurrentValue.EligibilityEngineService, null);

            builder.RegisterCustomerProfileClient(_appSettings.CurrentValue.CustomerProfileService, null);

            builder.RegisterWalletManagementClient(_appSettings.CurrentValue.WalletManagementService, null);

            builder.RegisterCampaignClient(_appSettings.CurrentValue.CampaignService, null);

            builder.RegisterType<BlockchainEventDecoder>()
                .As<IBlockchainEventDecoder>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .SingleInstance()
                .AutoActivate();

            builder.RegisterType<UndecodedEventHandler>()
                .As<IUndecodedEventHandler>()
                .SingleInstance();
        }
    }
}
