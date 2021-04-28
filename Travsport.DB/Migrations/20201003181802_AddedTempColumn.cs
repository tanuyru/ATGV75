using Microsoft.EntityFrameworkCore.Migrations;

namespace Travsport.DB.Migrations
{
    public partial class AddedTempColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TempId",
                table: "HorseStatsProfile",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TempId",
                table: "HorseStatsProfile");
        }
    }
}
