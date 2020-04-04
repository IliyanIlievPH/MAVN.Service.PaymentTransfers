using System;
using System.Threading.Tasks;
using System.Transactions;
using Lykke.Logs;
using MAVN.Service.PaymentTransfers.DomainServices.Common;
using Xunit;

namespace MAVN.Service.PaymentTransfers.Tests
{
    public class TransactionScopeHandlerTests
    {
        [Fact]
        public async Task WithTransactionAsync_TransactionAborted_Rethrown()
        {
            var sut = new TransactionScopeHandler(EmptyLogFactory.Instance);

            await Assert.ThrowsAsync<TransactionAbortedException>(() =>
                sut.WithTransactionAsync(() => throw new TransactionAbortedException()));
        }

        [Fact]
        public async Task WithTransactionAsyncWithResult_TransactionAborted_Rethrown()
        {
            var sut = new TransactionScopeHandler(EmptyLogFactory.Instance);

            await Assert.ThrowsAsync<TransactionAbortedException>(() =>
                sut.WithTransactionAsync<object>(() => throw new TransactionAbortedException()));
        }
    }
}
