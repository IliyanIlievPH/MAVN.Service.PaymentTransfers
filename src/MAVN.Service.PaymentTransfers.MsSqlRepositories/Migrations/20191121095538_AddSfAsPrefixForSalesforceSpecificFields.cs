using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.PaymentTransfers.MsSqlRepositories.Migrations
{
    public partial class AddSfAsPrefixForSalesforceSpecificFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "receipt_number",
                schema: "payment_transfers",
                table: "payment_transfers",
                newName: "sf_receipt_number");

            migrationBuilder.RenameColumn(
                name: "location_code",
                schema: "payment_transfers",
                table: "payment_transfers",
                newName: "sf_location_code");

            migrationBuilder.RenameColumn(
                name: "installment_type",
                schema: "payment_transfers",
                table: "payment_transfers",
                newName: "sf_installment_type");

            migrationBuilder.RenameColumn(
                name: "customer_trx_id",
                schema: "payment_transfers",
                table: "payment_transfers",
                newName: "sf_customer_trx_id");

            migrationBuilder.RenameColumn(
                name: "customer_account_number",
                schema: "payment_transfers",
                table: "payment_transfers",
                newName: "sf_customer_account_number");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "sf_receipt_number",
                schema: "payment_transfers",
                table: "payment_transfers",
                newName: "receipt_number");

            migrationBuilder.RenameColumn(
                name: "sf_location_code",
                schema: "payment_transfers",
                table: "payment_transfers",
                newName: "location_code");

            migrationBuilder.RenameColumn(
                name: "sf_installment_type",
                schema: "payment_transfers",
                table: "payment_transfers",
                newName: "installment_type");

            migrationBuilder.RenameColumn(
                name: "sf_customer_trx_id",
                schema: "payment_transfers",
                table: "payment_transfers",
                newName: "customer_trx_id");

            migrationBuilder.RenameColumn(
                name: "sf_customer_account_number",
                schema: "payment_transfers",
                table: "payment_transfers",
                newName: "customer_account_number");
        }
    }
}
