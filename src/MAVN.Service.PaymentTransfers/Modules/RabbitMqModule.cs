using Autofac;
using JetBrains.Annotations;
using Lykke.Job.QuorumTransactionWatcher.Contract;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using MAVN.Service.PaymentTransfers.Contract;
using MAVN.Service.PaymentTransfers.DomainServices.RabbitMq.Subscribers;
using MAVN.Service.PaymentTransfers.Settings;
using Lykke.SettingsReader;

namespace MAVN.Service.PaymentTransfers.Modules
{
    [UsedImplicitly]
    public class RabbitMqModule : Module
    {
        private const string PaymentTransferProcessedExchange = "lykke.wallet.transferprocessed";
        private const string PaymentTransferTokensReservedExchange = "lykke.wallet.transfertokensreserved";
        private const string DefaultQueueName = "paymenttransfers";

        private readonly RabbitMqSettings _settings;

        public RabbitMqModule(IReloadingManager<AppSettings> appSettings)
        {
            _settings = appSettings.CurrentValue.PaymentTransfersService.RabbitMq;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterJsonRabbitPublisher<PaymentTransferProcessedEvent>(
                _settings.RabbitMqConnectionString,
                PaymentTransferProcessedExchange);

            builder.RegisterJsonRabbitPublisher<PaymentTransferTokensReservedEvent>(
                _settings.RabbitMqConnectionString,
                PaymentTransferTokensReservedExchange);

            builder.RegisterJsonRabbitSubscriber<UndecodedSubscriber, UndecodedEvent>(
                _settings.RabbitMqConnectionString,
                Context.GetEndpointName<UndecodedEvent>(),
                DefaultQueueName);
        }
    }
}
