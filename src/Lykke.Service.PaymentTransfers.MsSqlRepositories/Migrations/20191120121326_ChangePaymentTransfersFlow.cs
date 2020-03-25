using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PaymentTransfers.MsSqlRepositories.Migrations
{
    public partial class ChangePaymentTransfersFlow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "amount",
                schema: "payment_transfers",
                table: "payment_transfers",
                newName: "receipt_number");

            migrationBuilder.AddColumn<decimal>(
                name: "amount_in_currency)",
                schema: "payment_transfers",
                table: "payment_transfers",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "amount_in_tokens)",
                schema: "payment_transfers",
                table: "payment_transfers",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "currency)",
                schema: "payment_transfers",
                table: "payment_transfers",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "customer_account_number",
                schema: "payment_transfers",
                table: "payment_transfers",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "customer_trx_id",
                schema: "payment_transfers",
                table: "payment_transfers",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "installment_type",
                schema: "payment_transfers",
                table: "payment_transfers",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "location_code",
                schema: "payment_transfers",
                table: "payment_transfers",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amount_in_currency)",
                schema: "payment_transfers",
                table: "payment_transfers");

            migrationBuilder.DropColumn(
                name: "amount_in_tokens)",
                schema: "payment_transfers",
                table: "payment_transfers");

            migrationBuilder.DropColumn(
                name: "currency)",
                schema: "payment_transfers",
                table: "payment_transfers");

            migrationBuilder.DropColumn(
                name: "customer_account_number",
                schema: "payment_transfers",
                table: "payment_transfers");

            migrationBuilder.DropColumn(
                name: "customer_trx_id",
                schema: "payment_transfers",
                table: "payment_transfers");

            migrationBuilder.DropColumn(
                name: "installment_type",
                schema: "payment_transfers",
                table: "payment_transfers");

            migrationBuilder.DropColumn(
                name: "location_code",
                schema: "payment_transfers",
                table: "payment_transfers");

            migrationBuilder.RenameColumn(
                name: "receipt_number",
                schema: "payment_transfers",
                table: "payment_transfers",
                newName: "amount");
        }
    }
}
