using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.PaymentTransfers.MsSqlRepositories.Migrations
{
    public partial class AddOrgIdAndRenameCampaignToSpendRule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "campaign_id",
                schema: "payment_transfers",
                table: "payment_transfers",
                newName: "spend_rule_id");

            migrationBuilder.AddColumn<string>(
                name: "sf_org_id",
                schema: "payment_transfers",
                table: "payment_transfers",
                nullable: false,
                defaultValue: "81");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sf_org_id",
                schema: "payment_transfers",
                table: "payment_transfers");

            migrationBuilder.RenameColumn(
                name: "spend_rule_id",
                schema: "payment_transfers",
                table: "payment_transfers",
                newName: "campaign_id");
        }
    }
}
