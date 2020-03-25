using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PaymentTransfers.MsSqlRepositories.Migrations
{
    public partial class AddPaymentTransfers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "payment_transfers");

            migrationBuilder.CreateTable(
                name: "payment_transfers",
                schema: "payment_transfers",
                columns: table => new
                {
                    transfer_id = table.Column<string>(nullable: false),
                    customer_id = table.Column<string>(nullable: false),
                    campaign_id = table.Column<string>(nullable: false),
                    invoice_id = table.Column<string>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    timestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_transfers", x => x.transfer_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payment_transfers_customer_id",
                schema: "payment_transfers",
                table: "payment_transfers",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transfers_status",
                schema: "payment_transfers",
                table: "payment_transfers",
                column: "status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_transfers",
                schema: "payment_transfers");
        }
    }
}
