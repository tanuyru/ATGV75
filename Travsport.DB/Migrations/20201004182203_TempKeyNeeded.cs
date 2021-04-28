using Microsoft.EntityFrameworkCore.Migrations;

namespace Travsport.DB.Migrations
{
    public partial class TempKeyNeeded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Keyed",
                table: "HorseStats",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Keyed",
                table: "HorseStats");
        }
    }
}
