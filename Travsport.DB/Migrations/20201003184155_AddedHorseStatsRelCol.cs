using Microsoft.EntityFrameworkCore.Migrations;

namespace Travsport.DB.Migrations
{
    public partial class AddedHorseStatsRelCol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RaceResult_HorseStatsId",
                table: "RaceResult");

            migrationBuilder.AddColumn<long>(
                name: "RaceResultId",
                table: "HorseStats",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_RaceResult_HorseStatsId",
                table: "RaceResult",
                column: "HorseStatsId");

            migrationBuilder.CreateIndex(
                name: "IX_HorseStats_RaceResultId",
                table: "HorseStats",
                column: "RaceResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_HorseStats_RaceResult_RaceResultId",
                table: "HorseStats",
                column: "RaceResultId",
                principalTable: "RaceResult",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HorseStats_RaceResult_RaceResultId",
                table: "HorseStats");

            migrationBuilder.DropIndex(
                name: "IX_RaceResult_HorseStatsId",
                table: "RaceResult");

            migrationBuilder.DropIndex(
                name: "IX_HorseStats_RaceResultId",
                table: "HorseStats");

            migrationBuilder.DropColumn(
                name: "RaceResultId",
                table: "HorseStats");

            migrationBuilder.CreateIndex(
                name: "IX_RaceResult_HorseStatsId",
                table: "RaceResult",
                column: "HorseStatsId",
                unique: true,
                filter: "[HorseStatsId] IS NOT NULL");
        }
    }
}
