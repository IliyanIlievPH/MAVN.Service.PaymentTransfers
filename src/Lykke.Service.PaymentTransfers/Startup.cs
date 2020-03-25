using System;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.PaymentTransfers.MappingProfiles;
using Lykke.Service.PaymentTransfers.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.PaymentTransfers
{
    [UsedImplicitly]
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "PaymentTransfers API",
            ApiVersion = "v1"
        };

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.SwaggerOptions = _swaggerOptions;

                options.Logs = logs =>
                {
                    logs.AzureTableName = "PaymentTransfersLog";
                    logs.AzureTableConnectionStringResolver = settings => settings.PaymentTransfersService.Db.LogsConnString;
                };

                options.Extend = (sc, st) =>
                {
                    sc.AddAutoMapper(typeof(AutoMapperProfile));
                };
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IMapper mapper)
        {
            app.UseLykkeConfiguration(options =>
            {
                options.SwaggerOptions = _swaggerOptions;
            });

            mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
