using System;
using System.Threading.Tasks;
using System.Transactions;
using Lykke.Common.MsSql;
using Lykke.Service.PaymentTransfers.Domain.Enums;
using Lykke.Service.PaymentTransfers.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Service.PaymentTransfers.MsSqlRepositories.Repositories
{
    public class PaymentTransfersRepository : IPaymentTransfersRepository
    {
        private readonly MsSqlContextFactory<PaymentTransfersContext> _contextFactory;

        public PaymentTransfersRepository(MsSqlContextFactory<PaymentTransfersContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task AddAsync(PaymentTransferDto paymentTransfer)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = PaymentTransferEntity.Create(paymentTransfer);

                context.PaymentTransfers.Add(entity);

                await context.SaveChangesAsync();
            }
        }

        public async Task<IPaymentTransfer> GetByTransferIdAsync(string transferId)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = await context.PaymentTransfers.FindAsync(transferId);

                return entity;
            }
        }

        public async Task SetStatusAsync(string transferId, PaymentTransferStatus status)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = new PaymentTransferEntity{TransferId = transferId};

                context.PaymentTransfers.Attach(entity);

                entity.Status = status;

                try
                {
                    await context.SaveChangesAsync();

                }
                catch (DbUpdateException)
                {
                    throw new InvalidOperationException("Entity was not found during status update");
                }
            }
        }

        public async Task<long> GetNextSequentialNumberAsync()
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            using (var context = _contextFactory.CreateDataContext())
            {
                try
                {
                    var sequentialNumberEntity = await context.InvoiceReceiptSequentialNumberEntities.FirstOrDefaultAsync();
                    if (sequentialNumberEntity == null)
                    {
                        sequentialNumberEntity = InvoiceReceiptSequentialNumberEntity.Create(1);
                        context.Add(sequentialNumberEntity);
                    }
                    else
                    {
                        sequentialNumberEntity.SequentialNumber += 1;
                        context.Update(sequentialNumberEntity);
                    }

                    await context.SaveChangesAsync();

                    return sequentialNumberEntity.SequentialNumber;
                }
                finally
                {
                    scope.Complete();
                }
            }
        }
    }
}
