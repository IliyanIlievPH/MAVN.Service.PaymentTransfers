using System.Threading.Tasks;

namespace MAVN.Service.PaymentTransfers.Domain.RabbitMq.Handlers
{
    public interface IUndecodedEventHandler
    {
        Task HandleAsync(string[] topics, string data, string contractAddress);
    }
}
