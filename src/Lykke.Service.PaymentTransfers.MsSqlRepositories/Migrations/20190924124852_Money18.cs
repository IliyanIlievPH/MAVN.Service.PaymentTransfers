using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PaymentTransfers.MsSqlRepositories.Migrations
{
    public partial class Money18 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "amount",
                schema: "payment_transfers",
                table: "payment_transfers",
                nullable: false,
                oldClrType: typeof(long));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "amount",
                schema: "payment_transfers",
                table: "payment_transfers",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
