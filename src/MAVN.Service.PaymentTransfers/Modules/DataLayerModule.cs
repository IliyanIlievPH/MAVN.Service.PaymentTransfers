using Autofac;
using JetBrains.Annotations;
using Lykke.Common.MsSql;
using MAVN.Service.PaymentTransfers.Domain.Models;
using MAVN.Service.PaymentTransfers.MsSqlRepositories;
using MAVN.Service.PaymentTransfers.MsSqlRepositories.Repositories;
using MAVN.Service.PaymentTransfers.Settings;
using Lykke.SettingsReader;

namespace MAVN.Service.PaymentTransfers.Modules
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
