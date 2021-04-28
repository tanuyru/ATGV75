using Microsoft.EntityFrameworkCore.Migrations;

namespace Travsport.DB.Migrations
{
    public partial class RemovedBrokenRel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Race_TrackDay_TrackDayId",
                table: "Race");

            migrationBuilder.DropIndex(
                name: "IX_Race_TrackDayId",
                table: "Race");

            migrationBuilder.DropColumn(
                name: "TrackDayId",
                table: "Race");

            migrationBuilder.AddColumn<double>(
                name: "ImpliedWinProb",
                table: "RaceResult",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImpliedWinProb",
                table: "RaceResult");

            migrationBuilder.AddColumn<long>(
                name: "TrackDayId",
                table: "Race",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Race_TrackDayId",
                table: "Race",
                column: "TrackDayId");

            migrationBuilder.AddForeignKey(
                name: "FK_Race_TrackDay_TrackDayId",
                table: "Race",
                column: "TrackDayId",
                principalTable: "TrackDay",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
