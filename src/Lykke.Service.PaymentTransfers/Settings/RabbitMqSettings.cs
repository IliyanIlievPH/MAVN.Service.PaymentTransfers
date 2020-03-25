using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PaymentTransfers.Settings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string RabbitMqConnectionString { get; set; }
    }
}
