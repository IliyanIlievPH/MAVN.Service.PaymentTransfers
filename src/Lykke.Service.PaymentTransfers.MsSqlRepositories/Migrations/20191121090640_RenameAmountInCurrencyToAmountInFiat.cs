using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PaymentTransfers.MsSqlRepositories.Migrations
{
    public partial class RenameAmountInCurrencyToAmountInFiat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "amount_in_currency)",
                schema: "payment_transfers",
                table: "payment_transfers",
                newName: "amount_in_fiat)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "amount_in_fiat)",
                schema: "payment_transfers",
                table: "payment_transfers",
                newName: "amount_in_currency)");
        }
    }
}
