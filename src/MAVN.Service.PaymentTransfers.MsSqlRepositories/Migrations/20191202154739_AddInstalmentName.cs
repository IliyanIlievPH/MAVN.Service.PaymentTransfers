using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.PaymentTransfers.MsSqlRepositories.Migrations
{
    public partial class AddInstalmentName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sf_instalment_name",
                schema: "payment_transfers",
                table: "payment_transfers",
                nullable: false,
                defaultValue: "DefaultInstalment");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sf_instalment_name",
                schema: "payment_transfers",
                table: "payment_transfers");
        }
    }
}
