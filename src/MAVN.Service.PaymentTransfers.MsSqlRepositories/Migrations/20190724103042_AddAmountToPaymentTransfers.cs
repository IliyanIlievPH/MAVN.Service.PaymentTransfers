using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.PaymentTransfers.MsSqlRepositories.Migrations
{
    public partial class AddAmountToPaymentTransfers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "amount",
                schema: "payment_transfers",
                table: "payment_transfers",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amount",
                schema: "payment_transfers",
                table: "payment_transfers");
        }
    }
}
