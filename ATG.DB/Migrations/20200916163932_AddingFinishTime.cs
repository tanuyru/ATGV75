using Microsoft.EntityFrameworkCore.Migrations;

namespace ATG.DB.Migrations
{
    public partial class AddingFinishTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EstimatedFinishTime",
                table: "RaceResult",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "FinishTimeMilliseconds",
                table: "RaceResult",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedFinishTime",
                table: "RaceResult");

            migrationBuilder.DropColumn(
                name: "FinishTimeMilliseconds",
                table: "RaceResult");
        }
    }
}
