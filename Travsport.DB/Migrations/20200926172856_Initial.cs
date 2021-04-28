using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Travsport.DB.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Arena",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    Condition = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arena", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AvailableGame",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduledStartTime = table.Column<DateTime>(nullable: false),
                    GameId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailableGame", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Breeder",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Location = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Breeder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComboGame",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Turnover = table.Column<long>(nullable: false),
                    Systems = table.Column<long>(nullable: false),
                    MissingRaceTimes = table.Column<bool>(nullable: false),
                    GameType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboGame", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Owner",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Owner", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Driver",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    BirthYear = table.Column<int>(nullable: false),
                    HomeArenaId = table.Column<long>(nullable: true),
                    Dummy = table.Column<string>(nullable: true),
                    ShortName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Driver", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Driver_Arena_HomeArenaId",
                        column: x => x.HomeArenaId,
                        principalTable: "Arena",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrackDay",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArenaId = table.Column<long>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    NumRaces = table.Column<int>(nullable: false),
                    NewStartList = table.Column<bool>(nullable: false),
                    OldStartList = table.Column<bool>(nullable: false),
                    BetType = table.Column<string>(nullable: true),
                    ShortBetType = table.Column<string>(nullable: true),
                    RaceDayGameTypes = table.Column<string>(nullable: true),
                    PrizeMoney = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackDay", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackDay_Arena_ArenaId",
                        column: x => x.ArenaId,
                        principalTable: "Arena",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainerDriver",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Location = table.Column<string>(nullable: true),
                    BirthYear = table.Column<int>(nullable: false),
                    HomeArenaId = table.Column<long>(nullable: true),
                    ShortName = table.Column<string>(nullable: true),
                    Linkable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainerDriver", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainerDriver_Arena_HomeArenaId",
                        column: x => x.HomeArenaId,
                        principalTable: "Arena",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GamePayout",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<long>(nullable: false),
                    NumWins = table.Column<int>(nullable: false),
                    Payout = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePayout", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GamePayout_ComboGame_GameId",
                        column: x => x.GameId,
                        principalTable: "ComboGame",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Horse",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    BirthYear = table.Column<DateTime>(nullable: true),
                    ArenaId = table.Column<long>(nullable: true),
                    Gender = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    Money = table.Column<double>(nullable: false),
                    Race = table.Column<string>(nullable: true),
                    Color = table.Column<string>(nullable: true),
                    StartPoints = table.Column<long>(nullable: false),
                    FatherHorseId = table.Column<long>(nullable: true),
                    MotherHorseId = table.Column<long>(nullable: true),
                    GrandFatherHorseId = table.Column<long>(nullable: true),
                    GrandMotherHorseId = table.Column<long>(nullable: true),
                    OwnerId = table.Column<long>(nullable: true),
                    TrainerId = table.Column<long>(nullable: true),
                    BreederId = table.Column<long>(nullable: true),
                    HistoryTimestamp = table.Column<DateTime>(nullable: true),
                    Linkable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Horse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Horse_Arena_ArenaId",
                        column: x => x.ArenaId,
                        principalTable: "Arena",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Horse_Breeder_BreederId",
                        column: x => x.BreederId,
                        principalTable: "Breeder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Horse_Horse_FatherHorseId",
                        column: x => x.FatherHorseId,
                        principalTable: "Horse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Horse_Horse_GrandFatherHorseId",
                        column: x => x.GrandFatherHorseId,
                        principalTable: "Horse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Horse_Horse_GrandMotherHorseId",
                        column: x => x.GrandMotherHorseId,
                        principalTable: "Horse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Horse_Horse_MotherHorseId",
                        column: x => x.MotherHorseId,
                        principalTable: "Horse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Horse_Owner_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Owner",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Horse_TrainerDriver_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "TrainerDriver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Race",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RaceId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Distance = table.Column<int>(nullable: false),
                    TrackCondition = table.Column<string>(nullable: true),
                    Sport = table.Column<string>(nullable: true),
                    ArenaId = table.Column<long>(nullable: true),
                    StartTime = table.Column<DateTime>(nullable: false),
                    ScheduledStartTime = table.Column<DateTime>(nullable: false),
                    RaceOrder = table.Column<int>(nullable: false),
                    StartType = table.Column<int>(nullable: false),
                    MediaId = table.Column<string>(nullable: true),
                    Leader500HorseId = table.Column<long>(nullable: true),
                    Leader1000HorseId = table.Column<long>(nullable: true),
                    First500KmTime = table.Column<long>(nullable: true),
                    First1000KmTime = table.Column<long>(nullable: true),
                    Last500KmTime = table.Column<long>(nullable: true),
                    WinnerFinishTime = table.Column<double>(nullable: false),
                    SystemsLostPercent = table.Column<double>(nullable: false),
                    SystemsLost = table.Column<long>(nullable: false),
                    TrackDayId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Race", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Race_Arena_ArenaId",
                        column: x => x.ArenaId,
                        principalTable: "Arena",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Race_Horse_Leader1000HorseId",
                        column: x => x.Leader1000HorseId,
                        principalTable: "Horse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Race_Horse_Leader500HorseId",
                        column: x => x.Leader500HorseId,
                        principalTable: "Horse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Race_TrackDay_TrackDayId",
                        column: x => x.TrackDayId,
                        principalTable: "TrackDay",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecentHorseStart",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HorseId = table.Column<long>(nullable: false),
                    KmTimeMilliseconds = table.Column<long>(nullable: false),
                    Galloped = table.Column<bool>(nullable: false),
                    DQ = table.Column<bool>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    RaceId = table.Column<string>(nullable: true),
                    Distance = table.Column<long>(nullable: false),
                    Sport = table.Column<string>(nullable: true),
                    StartMethod = table.Column<int>(nullable: false),
                    Track = table.Column<int>(nullable: false),
                    WinOdds = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecentHorseStart", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecentHorseStart_Horse_HorseId",
                        column: x => x.HorseId,
                        principalTable: "Horse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameRace",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<long>(nullable: false),
                    RaceId = table.Column<long>(nullable: false),
                    GameRaceIndex = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameRace", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameRace_ComboGame_GameId",
                        column: x => x.GameId,
                        principalTable: "ComboGame",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameRace_Race_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Race",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaceResult",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinishPosition = table.Column<int>(nullable: false),
                    HorseId = table.Column<long>(nullable: false),
                    TrainerId = table.Column<long>(nullable: true),
                    RaceFKId = table.Column<long>(nullable: false),
                    DriverId = table.Column<long>(nullable: true),
                    Sulky = table.Column<string>(nullable: true),
                    StartNumber = table.Column<int>(nullable: false),
                    PositionForDistance = table.Column<int>(nullable: false),
                    KmTimeMilliSeconds = table.Column<long>(nullable: false),
                    WinOdds = table.Column<double>(nullable: false),
                    PlatsOdds = table.Column<double>(nullable: false),
                    WonPrizeMoney = table.Column<int>(nullable: false),
                    DQ = table.Column<bool>(nullable: false),
                    Galopp = table.Column<bool>(nullable: false),
                    FrontShoes = table.Column<bool>(nullable: true),
                    BackShoes = table.Column<bool>(nullable: true),
                    FrontChange = table.Column<bool>(nullable: true),
                    BackChange = table.Column<bool>(nullable: true),
                    Scratched = table.Column<bool>(nullable: false),
                    DriverChanged = table.Column<bool>(nullable: true),
                    FinishTimeMilliseconds = table.Column<double>(nullable: false),
                    FinishTimeAfterWinner = table.Column<double>(nullable: false),
                    Distance = table.Column<int>(nullable: false),
                    DistanceHandicap = table.Column<int>(nullable: false),
                    Distribution = table.Column<double>(nullable: false),
                    SmallDistribution = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaceResult_TrainerDriver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "TrainerDriver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RaceResult_Horse_HorseId",
                        column: x => x.HorseId,
                        principalTable: "Horse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaceResult_Race_RaceFKId",
                        column: x => x.RaceFKId,
                        principalTable: "Race",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaceResult_TrainerDriver_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "TrainerDriver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameDistribution",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<long>(nullable: false),
                    RaceResultId = table.Column<long>(nullable: false),
                    Distribution = table.Column<double>(nullable: false),
                    SystemsLeft = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameDistribution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameDistribution_ComboGame_GameId",
                        column: x => x.GameId,
                        principalTable: "ComboGame",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameDistribution_RaceResult_RaceResultId",
                        column: x => x.RaceResultId,
                        principalTable: "RaceResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvailableGame_GameId",
                table: "AvailableGame",
                column: "GameId",
                unique: true,
                filter: "[GameId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Driver_HomeArenaId",
                table: "Driver",
                column: "HomeArenaId");

            migrationBuilder.CreateIndex(
                name: "IX_GameDistribution_GameId",
                table: "GameDistribution",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameDistribution_RaceResultId",
                table: "GameDistribution",
                column: "RaceResultId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePayout_GameId",
                table: "GamePayout",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameRace_GameId",
                table: "GameRace",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameRace_RaceId",
                table: "GameRace",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Horse_ArenaId",
                table: "Horse",
                column: "ArenaId");

            migrationBuilder.CreateIndex(
                name: "IX_Horse_BreederId",
                table: "Horse",
                column: "BreederId");

            migrationBuilder.CreateIndex(
                name: "IX_Horse_FatherHorseId",
                table: "Horse",
                column: "FatherHorseId");

            migrationBuilder.CreateIndex(
                name: "IX_Horse_GrandFatherHorseId",
                table: "Horse",
                column: "GrandFatherHorseId");

            migrationBuilder.CreateIndex(
                name: "IX_Horse_GrandMotherHorseId",
                table: "Horse",
                column: "GrandMotherHorseId");

            migrationBuilder.CreateIndex(
                name: "IX_Horse_MotherHorseId",
                table: "Horse",
                column: "MotherHorseId");

            migrationBuilder.CreateIndex(
                name: "IX_Horse_Name",
                table: "Horse",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Horse_OwnerId",
                table: "Horse",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Horse_TrainerId",
                table: "Horse",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_Race_ArenaId",
                table: "Race",
                column: "ArenaId");

            migrationBuilder.CreateIndex(
                name: "IX_Race_Leader1000HorseId",
                table: "Race",
                column: "Leader1000HorseId");

            migrationBuilder.CreateIndex(
                name: "IX_Race_Leader500HorseId",
                table: "Race",
                column: "Leader500HorseId");

            migrationBuilder.CreateIndex(
                name: "IX_Race_RaceId",
                table: "Race",
                column: "RaceId",
                unique: true,
                filter: "[RaceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Race_TrackDayId",
                table: "Race",
                column: "TrackDayId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceResult_DriverId",
                table: "RaceResult",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceResult_HorseId",
                table: "RaceResult",
                column: "HorseId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceResult_RaceFKId",
                table: "RaceResult",
                column: "RaceFKId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceResult_TrainerId",
                table: "RaceResult",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_RecentHorseStart_HorseId",
                table: "RecentHorseStart",
                column: "HorseId");

            migrationBuilder.CreateIndex(
                name: "IX_RecentHorseStart_RaceId",
                table: "RecentHorseStart",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackDay_ArenaId",
                table: "TrackDay",
                column: "ArenaId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerDriver_HomeArenaId",
                table: "TrainerDriver",
                column: "HomeArenaId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerDriver_ShortName",
                table: "TrainerDriver",
                column: "ShortName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvailableGame");

            migrationBuilder.DropTable(
                name: "Driver");

            migrationBuilder.DropTable(
                name: "GameDistribution");

            migrationBuilder.DropTable(
                name: "GamePayout");

            migrationBuilder.DropTable(
                name: "GameRace");

            migrationBuilder.DropTable(
                name: "RecentHorseStart");

            migrationBuilder.DropTable(
                name: "RaceResult");

            migrationBuilder.DropTable(
                name: "ComboGame");

            migrationBuilder.DropTable(
                name: "Race");

            migrationBuilder.DropTable(
                name: "Horse");

            migrationBuilder.DropTable(
                name: "TrackDay");

            migrationBuilder.DropTable(
                name: "Breeder");

            migrationBuilder.DropTable(
                name: "Owner");

            migrationBuilder.DropTable(
                name: "TrainerDriver");

            migrationBuilder.DropTable(
                name: "Arena");
        }
    }
}
