using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Travsport.DB.Migrations
{
    public partial class AddedCalculatedStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "HorseStatsId",
                table: "RaceResult",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DetailStatsVersion",
                table: "Race",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "HorseStatsProfile",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    First500Factor = table.Column<float>(nullable: false),
                    First1000Factor = table.Column<float>(nullable: false),
                    Last500Factor = table.Column<float>(nullable: false),
                    ShortDistanceFactor = table.Column<float>(nullable: false),
                    MediumDistanceFactor = table.Column<float>(nullable: false),
                    LongDistanceFactor = table.Column<float>(nullable: false),
                    AutoStartFactor = table.Column<float>(nullable: false),
                    VoltStartFactor = table.Column<float>(nullable: false),
                    TrackConditionFactor = table.Column<float>(nullable: false),
                    LightTrackConditionFactor = table.Column<float>(nullable: false),
                    HeavierTrackConditionFactor = table.Column<float>(nullable: false),
                    HeavyTrackConditionFactor = table.Column<float>(nullable: false),
                    WinterTrackConditionFactor = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorseStatsProfile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HorseStats",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HorseId = table.Column<long>(nullable: false),
                    RaceDate = table.Column<DateTime>(nullable: false),
                    BestShortAuto = table.Column<long>(nullable: true),
                    BestShortVolt = table.Column<long>(nullable: true),
                    BestShortTime = table.Column<long>(nullable: true),
                    BestMediumAuto = table.Column<long>(nullable: true),
                    BestMediumVolt = table.Column<long>(nullable: true),
                    BestMediumTime = table.Column<long>(nullable: true),
                    BestLongAuto = table.Column<long>(nullable: true),
                    BestLongVolt = table.Column<long>(nullable: true),
                    BestLongTime = table.Column<long>(nullable: true),
                    BestAuto = table.Column<long>(nullable: true),
                    BestVolt = table.Column<long>(nullable: true),
                    Best = table.Column<long>(nullable: true),
                    BestLastMonthShort = table.Column<long>(nullable: true),
                    BestLastMonthMedium = table.Column<long>(nullable: true),
                    BestLastMonthLong = table.Column<long>(nullable: true),
                    BestLastMonth = table.Column<long>(nullable: true),
                    MedianLongAuto = table.Column<long>(nullable: true),
                    MedianLongVolt = table.Column<long>(nullable: true),
                    MedianLongTime = table.Column<long>(nullable: true),
                    MedianShortAuto = table.Column<long>(nullable: true),
                    MedianShortVolt = table.Column<long>(nullable: true),
                    MedianShortTime = table.Column<long>(nullable: true),
                    MedianMediumAuto = table.Column<long>(nullable: true),
                    MedianMediumVolt = table.Column<long>(nullable: true),
                    MedianMediumTime = table.Column<long>(nullable: true),
                    MedianAuto = table.Column<long>(nullable: true),
                    MedianVolt = table.Column<long>(nullable: true),
                    Median = table.Column<long>(nullable: true),
                    MedianLastMonthShort = table.Column<long>(nullable: true),
                    MedianLastMonthMedium = table.Column<long>(nullable: true),
                    MedianLastMonthLong = table.Column<long>(nullable: true),
                    MedianLastMonth = table.Column<long>(nullable: true),
                    AverageMediumAuto = table.Column<long>(nullable: true),
                    AverageMediumVolt = table.Column<long>(nullable: true),
                    AverageMediumTime = table.Column<long>(nullable: true),
                    AverageLongAuto = table.Column<long>(nullable: true),
                    AverageLongVolt = table.Column<long>(nullable: true),
                    AverageLongTime = table.Column<long>(nullable: true),
                    AverageShortAuto = table.Column<long>(nullable: true),
                    AverageShortVolt = table.Column<long>(nullable: true),
                    AverageShortTime = table.Column<long>(nullable: true),
                    AverageAuto = table.Column<long>(nullable: true),
                    AverageVolt = table.Column<long>(nullable: true),
                    Average = table.Column<long>(nullable: true),
                    AverageLastMonthShort = table.Column<long>(nullable: true),
                    AverageLastMonthMedium = table.Column<long>(nullable: true),
                    AverageLastMonthLong = table.Column<long>(nullable: true),
                    AverageLastMonth = table.Column<long>(nullable: true),
                    NumShortAuto = table.Column<int>(nullable: false),
                    NumShortVolt = table.Column<int>(nullable: false),
                    NumMediumAuto = table.Column<int>(nullable: false),
                    NumMediumVolt = table.Column<int>(nullable: false),
                    NumLongAuto = table.Column<int>(nullable: false),
                    NumLongVolt = table.Column<int>(nullable: false),
                    NumShort = table.Column<int>(nullable: false),
                    NumMedium = table.Column<int>(nullable: false),
                    NumLong = table.Column<int>(nullable: false),
                    NumVolts = table.Column<int>(nullable: false),
                    NumAutos = table.Column<int>(nullable: false),
                    NumShortLastMonth = table.Column<int>(nullable: false),
                    NumMediumLastMonth = table.Column<int>(nullable: false),
                    NumLongLastMonth = table.Column<int>(nullable: false),
                    NumLastMonth = table.Column<int>(nullable: false),
                    NumShape = table.Column<int>(nullable: false),
                    NumTotals = table.Column<int>(nullable: false),
                    WinPercent = table.Column<float>(nullable: false),
                    Top3Percent = table.Column<float>(nullable: false),
                    PlacePercent = table.Column<float>(nullable: false),
                    MoneyPerStartShape = table.Column<float>(nullable: false),
                    MoneyPerStart = table.Column<float>(nullable: false),
                    WinShape = table.Column<float>(nullable: false),
                    PlaceShape = table.Column<float>(nullable: false),
                    Top3Shape = table.Column<float>(nullable: false),
                    TimeAfterWinnerShapeAverage = table.Column<float>(nullable: false),
                    TimeAfterWinnerShapeMedian = table.Column<float>(nullable: false),
                    TimeAfterWinnerLast = table.Column<float>(nullable: false),
                    TimeAfterWinnerMedian = table.Column<float>(nullable: false),
                    TimeAfterWinnerAverage = table.Column<float>(nullable: false),
                    NumDriverHistory = table.Column<int>(nullable: false),
                    DriverWinPercent = table.Column<float>(nullable: false),
                    DriverTop3Percent = table.Column<float>(nullable: false),
                    DriverPlacePercent = table.Column<float>(nullable: false),
                    DriverMoneyPerStart = table.Column<float>(nullable: false),
                    TimeAfterWinnerLastCapProfileId = table.Column<long>(nullable: true),
                    TimeAfterWinnerPlaceCapProfileId = table.Column<long>(nullable: true),
                    TimeAfterWinnerMinMaxProfileId = table.Column<long>(nullable: true),
                    TimeAfterWinnerNormMinPlacedCapProfileId = table.Column<long>(nullable: true),
                    KmTimeValidProfileId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorseStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorseStats_HorseStatsProfile_KmTimeValidProfileId",
                        column: x => x.KmTimeValidProfileId,
                        principalTable: "HorseStatsProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HorseStats_HorseStatsProfile_TimeAfterWinnerLastCapProfileId",
                        column: x => x.TimeAfterWinnerLastCapProfileId,
                        principalTable: "HorseStatsProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HorseStats_HorseStatsProfile_TimeAfterWinnerMinMaxProfileId",
                        column: x => x.TimeAfterWinnerMinMaxProfileId,
                        principalTable: "HorseStatsProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HorseStats_HorseStatsProfile_TimeAfterWinnerNormMinPlacedCapProfileId",
                        column: x => x.TimeAfterWinnerNormMinPlacedCapProfileId,
                        principalTable: "HorseStatsProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HorseStats_HorseStatsProfile_TimeAfterWinnerPlaceCapProfileId",
                        column: x => x.TimeAfterWinnerPlaceCapProfileId,
                        principalTable: "HorseStatsProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RaceResult_HorseStatsId",
                table: "RaceResult",
                column: "HorseStatsId",
                unique: true,
                filter: "[HorseStatsId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HorseStats_KmTimeValidProfileId",
                table: "HorseStats",
                column: "KmTimeValidProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HorseStats_TimeAfterWinnerLastCapProfileId",
                table: "HorseStats",
                column: "TimeAfterWinnerLastCapProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HorseStats_TimeAfterWinnerMinMaxProfileId",
                table: "HorseStats",
                column: "TimeAfterWinnerMinMaxProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HorseStats_TimeAfterWinnerNormMinPlacedCapProfileId",
                table: "HorseStats",
                column: "TimeAfterWinnerNormMinPlacedCapProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HorseStats_TimeAfterWinnerPlaceCapProfileId",
                table: "HorseStats",
                column: "TimeAfterWinnerPlaceCapProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_RaceResult_HorseStats_HorseStatsId",
                table: "RaceResult",
                column: "HorseStatsId",
                principalTable: "HorseStats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RaceResult_HorseStats_HorseStatsId",
                table: "RaceResult");

            migrationBuilder.DropTable(
                name: "HorseStats");

            migrationBuilder.DropTable(
                name: "HorseStatsProfile");

            migrationBuilder.DropIndex(
                name: "IX_RaceResult_HorseStatsId",
                table: "RaceResult");

            migrationBuilder.DropColumn(
                name: "HorseStatsId",
                table: "RaceResult");

            migrationBuilder.DropColumn(
                name: "DetailStatsVersion",
                table: "Race");
        }
    }
}
