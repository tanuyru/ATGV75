using Microsoft.EntityFrameworkCore.Migrations;

namespace Travsport.DB.Migrations
{
    public partial class AddedColumnsStuff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RaceDayId",
                table: "Race",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RaceDayId",
                table: "Race");
        }
    }
}
