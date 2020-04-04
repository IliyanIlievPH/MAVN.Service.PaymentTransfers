using System;
using System.Threading.Tasks;

namespace MAVN.Service.PaymentTransfers.Domain.Common
{
    public interface ITransactionScopeHandler
    {
        Task<T> WithTransactionAsync<T>(Func<Task<T>> func);
        Task WithTransactionAsync(Func<Task> action);
    }
}
