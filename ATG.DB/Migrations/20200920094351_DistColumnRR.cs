using Microsoft.EntityFrameworkCore.Migrations;

namespace ATG.DB.Migrations
{
    public partial class DistColumnRR : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Distribution",
                table: "RaceResult",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Distribution",
                table: "RaceResult");
        }
    }
}
