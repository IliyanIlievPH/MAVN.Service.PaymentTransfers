using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAVN.Service.PaymentTransfers.MsSqlRepositories
{
    [Table("invoice_receipt_sequential_numbers")]
    public class InvoiceReceiptSequentialNumberEntity
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("sequential_number")]
        [Required]
        public long SequentialNumber { get; set; }

        public static InvoiceReceiptSequentialNumberEntity Create(long number)
        {
            return new InvoiceReceiptSequentialNumberEntity
            {
                SequentialNumber = number
            };
        }
    }
}
