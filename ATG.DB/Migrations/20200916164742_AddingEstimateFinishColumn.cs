using Microsoft.EntityFrameworkCore.Migrations;

namespace ATG.DB.Migrations
{
    public partial class AddingEstimateFinishColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "FinishTimeAfterWinner",
                table: "RaceResult",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinishTimeAfterWinner",
                table: "RaceResult");
        }
    }
}
