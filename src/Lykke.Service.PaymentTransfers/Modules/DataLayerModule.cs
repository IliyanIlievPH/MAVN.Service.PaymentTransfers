using Autofac;
using JetBrains.Annotations;
using Lykke.Common.MsSql;
using Lykke.Service.PaymentTransfers.Domain.Models;
using Lykke.Service.PaymentTransfers.MsSqlRepositories;
using Lykke.Service.PaymentTransfers.MsSqlRepositories.Repositories;
using Lykke.Service.PaymentTransfers.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.PaymentTransfers.Modules
{
    [UsedImplicitly]
    public class DataLayerModule : Module
    {
        private readonly DbSettings _dbSettings;

        public DataLayerModule(IReloadingManager<AppSettings> appSettings)
        {
            _dbSettings = appSettings.CurrentValue.PaymentTransfersService.Db;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMsSql(
                _dbSettings.DataConnString,
                connString => new PaymentTransfersContext(connString, false),
                dbConn => new PaymentTransfersContext(dbConn));

            builder.RegisterType<PaymentTransfersRepository>()
                .As<IPaymentTransfersRepository>()
                .SingleInstance();
        }
    }
}
