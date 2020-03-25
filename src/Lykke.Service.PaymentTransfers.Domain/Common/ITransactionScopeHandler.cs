﻿using System;
using System.Threading.Tasks;

namespace Lykke.Service.PaymentTransfers.Domain.Common
{
    public interface ITransactionScopeHandler
    {
        Task<T> WithTransactionAsync<T>(Func<Task<T>> func);
        Task WithTransactionAsync(Func<Task> action);
    }
}
