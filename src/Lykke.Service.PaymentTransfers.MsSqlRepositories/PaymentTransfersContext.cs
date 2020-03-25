using System.Data.Common;
using JetBrains.Annotations;
using Lykke.Common.MsSql;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Service.PaymentTransfers.MsSqlRepositories
{
    public class PaymentTransfersContext : MsSqlContext
    {
        private const string Schema = "payment_transfers";

        internal DbSet<PaymentTransferEntity> PaymentTransfers { get; set; }
        internal DbSet<InvoiceReceiptSequentialNumberEntity> InvoiceReceiptSequentialNumberEntities { get; set; }

        // C-tor for EF migrations
        [UsedImplicitly]
        public PaymentTransfersContext()
            : base(Schema)
        {
        }

        public PaymentTransfersContext(string connectionString, bool isTraceEnabled)
            : base(Schema, connectionString, isTraceEnabled)
        {
        }

        public PaymentTransfersContext(DbConnection dbConnection)
            : base(Schema, dbConnection)
        {
        }

        protected override void OnLykkeModelCreating(ModelBuilder modelBuilder)
        {
            var paymentTransfersBuilder = modelBuilder.Entity<PaymentTransferEntity>();
            paymentTransfersBuilder.HasIndex(x => x.CustomerId).IsUnique(false);
            paymentTransfersBuilder.HasIndex(x => x.Status).IsUnique(false);
        }
    }
}
