using Microsoft.EntityFrameworkCore.Migrations;

namespace Travsport.DB.Migrations
{
    public partial class AddingMoreColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "DriverMoneyPerStartAuto",
                table: "HorseStats",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "DriverMoneyPerStartVolt",
                table: "HorseStats",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "DriverPlacePercentAuto",
                table: "HorseStats",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "DriverPlacePercentVolt",
                table: "HorseStats",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "DriverTop3PercentAuto",
                table: "HorseStats",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "DriverTop3PercentVolt",
                table: "HorseStats",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "DriverWinPercentAuto",
                table: "HorseStats",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "DriverWinPercentVolt",
                table: "HorseStats",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "NumDriverHistoryAuto",
                table: "HorseStats",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumDriverHistoryVolt",
                table: "HorseStats",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumHeavier",
                table: "HorseStats",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumHeavy",
                table: "HorseStats",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumLight",
                table: "HorseStats",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumWinter",
                table: "HorseStats",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverMoneyPerStartAuto",
                table: "HorseStats");

            migrationBuilder.DropColumn(
                name: "DriverMoneyPerStartVolt",
                table: "HorseStats");

            migrationBuilder.DropColumn(
                name: "DriverPlacePercentAuto",
                table: "HorseStats");

            migrationBuilder.DropColumn(
                name: "DriverPlacePercentVolt",
                table: "HorseStats");

            migrationBuilder.DropColumn(
                name: "DriverTop3PercentAuto",
                table: "HorseStats");

            migrationBuilder.DropColumn(
                name: "DriverTop3PercentVolt",
                table: "HorseStats");

            migrationBuilder.DropColumn(
                name: "DriverWinPercentAuto",
                table: "HorseStats");

            migrationBuilder.DropColumn(
                name: "DriverWinPercentVolt",
                table: "HorseStats");

            migrationBuilder.DropColumn(
                name: "NumDriverHistoryAuto",
                table: "HorseStats");

            migrationBuilder.DropColumn(
                name: "NumDriverHistoryVolt",
                table: "HorseStats");

            migrationBuilder.DropColumn(
                name: "NumHeavier",
                table: "HorseStats");

            migrationBuilder.DropColumn(
                name: "NumHeavy",
                table: "HorseStats");

            migrationBuilder.DropColumn(
                name: "NumLight",
                table: "HorseStats");

            migrationBuilder.DropColumn(
                name: "NumWinter",
                table: "HorseStats");
        }
    }
}
