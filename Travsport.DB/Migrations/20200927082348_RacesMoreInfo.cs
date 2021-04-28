using Microsoft.EntityFrameworkCore.Migrations;

namespace Travsport.DB.Migrations
{
    public partial class RacesMoreInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Horse_Name",
                table: "Horse");

            migrationBuilder.AddColumn<double>(
                name: "NormalizedFinishTime",
                table: "RaceResult",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "NormalizedFinishTimesPlaced",
                table: "RaceResult",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "First1000Handicap",
                table: "Race",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "First1000Position",
                table: "Race",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "First1000SpeedRatio",
                table: "Race",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "First500Handicap",
                table: "Race",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "First500Position",
                table: "Race",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "First500SpeedRatio",
                table: "Race",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Last500SpeedRatio",
                table: "Race",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastFinishTime",
                table: "Race",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LastPlaceFinishTime",
                table: "Race",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "WinnerDriverId",
                table: "Race",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "WinnerHorseId",
                table: "Race",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WinnerKmTimeMilliseconds",
                table: "Race",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "WinnerTrainerId",
                table: "Race",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Horse_Name",
                table: "Horse",
                column: "Name",
                unique: false,
                filter: "[Name] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Horse_Name",
                table: "Horse");

            migrationBuilder.DropColumn(
                name: "NormalizedFinishTime",
                table: "RaceResult");

            migrationBuilder.DropColumn(
                name: "NormalizedFinishTimesPlaced",
                table: "RaceResult");

            migrationBuilder.DropColumn(
                name: "First1000Handicap",
                table: "Race");

            migrationBuilder.DropColumn(
                name: "First1000Position",
                table: "Race");

            migrationBuilder.DropColumn(
                name: "First1000SpeedRatio",
                table: "Race");

            migrationBuilder.DropColumn(
                name: "First500Handicap",
                table: "Race");

            migrationBuilder.DropColumn(
                name: "First500Position",
                table: "Race");

            migrationBuilder.DropColumn(
                name: "First500SpeedRatio",
                table: "Race");

            migrationBuilder.DropColumn(
                name: "Last500SpeedRatio",
                table: "Race");

            migrationBuilder.DropColumn(
                name: "LastFinishTime",
                table: "Race");

            migrationBuilder.DropColumn(
                name: "LastPlaceFinishTime",
                table: "Race");

            migrationBuilder.DropColumn(
                name: "WinnerDriverId",
                table: "Race");

            migrationBuilder.DropColumn(
                name: "WinnerHorseId",
                table: "Race");

            migrationBuilder.DropColumn(
                name: "WinnerKmTimeMilliseconds",
                table: "Race");

            migrationBuilder.DropColumn(
                name: "WinnerTrainerId",
                table: "Race");

            migrationBuilder.CreateIndex(
                name: "IX_Horse_Name",
                table: "Horse",
                column: "Name",
                filter: "[Name] IS NOT NULL");
        }
    }
}
