using Microsoft.EntityFrameworkCore.Migrations;

namespace ATG.DB.Migrations
{
    public partial class Initial2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "HorseTotalMoney",
                table: "RaceResult",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "HorseTotalPlacepercent",
                table: "RaceResult",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "HorseTotalWinPercent",
                table: "RaceResult",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HorseTotalMoney",
                table: "RaceResult");

            migrationBuilder.DropColumn(
                name: "HorseTotalPlacepercent",
                table: "RaceResult");

            migrationBuilder.DropColumn(
                name: "HorseTotalWinPercent",
                table: "RaceResult");
        }
    }
}
