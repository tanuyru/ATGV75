using Microsoft.EntityFrameworkCore.Migrations;

namespace Travsport.DB.Migrations
{
    public partial class RacesMoreInfo2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "StartSpeedFigure",
                table: "Race",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartSpeedFigure",
                table: "Race");
        }
    }
}
