using Microsoft.EntityFrameworkCore.Migrations;

namespace ATG.DB.Migrations
{
    public partial class AddedMoreData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Track",
                table: "RecentHorseStart",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "WinOdds",
                table: "RecentHorseStart",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Distance",
                table: "RaceResult",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DistanceHandicap",
                table: "RaceResult",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "MoneyPerStartTotal",
                table: "RaceResult",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "SystemsLost",
                table: "Race",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<double>(
                name: "SystemsLostPercent",
                table: "Race",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WinnerFinishTime",
                table: "Race",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<string>(
                name: "GameId",
                table: "AvailableGame",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AvailableGame_GameId",
                table: "AvailableGame",
                column: "GameId",
                unique: true,
                filter: "[GameId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AvailableGame_GameId",
                table: "AvailableGame");

            migrationBuilder.DropColumn(
                name: "Track",
                table: "RecentHorseStart");

            migrationBuilder.DropColumn(
                name: "WinOdds",
                table: "RecentHorseStart");

            migrationBuilder.DropColumn(
                name: "Distance",
                table: "RaceResult");

            migrationBuilder.DropColumn(
                name: "DistanceHandicap",
                table: "RaceResult");

            migrationBuilder.DropColumn(
                name: "MoneyPerStartTotal",
                table: "RaceResult");

            migrationBuilder.DropColumn(
                name: "SystemsLost",
                table: "Race");

            migrationBuilder.DropColumn(
                name: "SystemsLostPercent",
                table: "Race");

            migrationBuilder.DropColumn(
                name: "WinnerFinishTime",
                table: "Race");

            migrationBuilder.AlterColumn<string>(
                name: "GameId",
                table: "AvailableGame",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
