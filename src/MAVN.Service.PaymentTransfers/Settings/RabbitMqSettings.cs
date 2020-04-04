using Lykke.SettingsReader.Attributes;

namespace MAVN.Service.PaymentTransfers.Settings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string RabbitMqConnectionString { get; set; }
    }
}
