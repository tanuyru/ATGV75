﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Travsport.DB;

namespace Travsport.DB.Migrations
{
    [DbContext(typeof(TravsportContext))]
    [Migration("20201001161440_AddedColumnsStuff")]
    partial class AddedColumnsStuff
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Travsport.DB.Entities.Arena", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Condition")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Country")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Arena");
                });

            modelBuilder.Entity("Travsport.DB.Entities.AvailableGame", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("GameId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("ScheduledStartTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("GameId")
                        .IsUnique()
                        .HasFilter("[GameId] IS NOT NULL");

                    b.ToTable("AvailableGame");
                });

            modelBuilder.Entity("Travsport.DB.Entities.Breeder", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<string>("Location")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Breeder");
                });

            modelBuilder.Entity("Travsport.DB.Entities.ComboGame", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("GameId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("GameType")
                        .HasColumnType("int");

                    b.Property<bool>("MissingRaceTimes")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Systems")
                        .HasColumnType("bigint");

                    b.Property<long>("Turnover")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("ComboGame");
                });

            modelBuilder.Entity("Travsport.DB.Entities.Driver", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<int>("BirthYear")
                        .HasColumnType("int");

                    b.Property<string>("Dummy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("HomeArenaId")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ShortName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("HomeArenaId");

                    b.ToTable("Driver");
                });

            modelBuilder.Entity("Travsport.DB.Entities.GameDistribution", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<double>("Distribution")
                        .HasColumnType("float");

                    b.Property<long>("GameId")
                        .HasColumnType("bigint");

                    b.Property<long>("RaceResultId")
                        .HasColumnType("bigint");

                    b.Property<int>("SystemsLeft")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.HasIndex("RaceResultId");

                    b.ToTable("GameDistribution");
                });

            modelBuilder.Entity("Travsport.DB.Entities.GamePayout", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("GameId")
                        .HasColumnType("bigint");

                    b.Property<int>("NumWins")
                        .HasColumnType("int");

                    b.Property<double>("Payout")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("GamePayout");
                });

            modelBuilder.Entity("Travsport.DB.Entities.GameRace", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("GameId")
                        .HasColumnType("bigint");

                    b.Property<int>("GameRaceIndex")
                        .HasColumnType("int");

                    b.Property<long>("RaceId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.HasIndex("RaceId");

                    b.ToTable("GameRace");
                });

            modelBuilder.Entity("Travsport.DB.Entities.Horse", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<long?>("ArenaId")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("BirthYear")
                        .HasColumnType("datetime2");

                    b.Property<long?>("BreederId")
                        .HasColumnType("bigint");

                    b.Property<string>("Color")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Country")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("FatherHorseId")
                        .HasColumnType("bigint");

                    b.Property<string>("Gender")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("GrandFatherHorseId")
                        .HasColumnType("bigint");

                    b.Property<long?>("GrandMotherHorseId")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("HistoryTimestamp")
                        .HasColumnType("datetime2");

                    b.Property<bool>("Linkable")
                        .HasColumnType("bit");

                    b.Property<double>("Money")
                        .HasColumnType("float");

                    b.Property<long?>("MotherHorseId")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<long?>("OwnerId")
                        .HasColumnType("bigint");

                    b.Property<string>("Race")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("StartPoints")
                        .HasColumnType("bigint");

                    b.Property<long?>("TrainerId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("ArenaId");

                    b.HasIndex("BreederId");

                    b.HasIndex("FatherHorseId");

                    b.HasIndex("GrandFatherHorseId");

                    b.HasIndex("GrandMotherHorseId");

                    b.HasIndex("MotherHorseId");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasFilter("[Name] IS NOT NULL");

                    b.HasIndex("OwnerId");

                    b.HasIndex("TrainerId");

                    b.ToTable("Horse");
                });

            modelBuilder.Entity("Travsport.DB.Entities.Owner", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<string>("Country")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Owner");
                });

            modelBuilder.Entity("Travsport.DB.Entities.Race", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long?>("ArenaId")
                        .HasColumnType("bigint");

                    b.Property<int>("Distance")
                        .HasColumnType("int");

                    b.Property<int?>("First1000Handicap")
                        .HasColumnType("int");

                    b.Property<long?>("First1000KmTime")
                        .HasColumnType("bigint");

                    b.Property<int?>("First1000Position")
                        .HasColumnType("int");

                    b.Property<double?>("First1000SpeedRatio")
                        .HasColumnType("float");

                    b.Property<int?>("First500Handicap")
                        .HasColumnType("int");

                    b.Property<long?>("First500KmTime")
                        .HasColumnType("bigint");

                    b.Property<int?>("First500Position")
                        .HasColumnType("int");

                    b.Property<double?>("First500SpeedRatio")
                        .HasColumnType("float");

                    b.Property<string>("InvalidReason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("Last500KmTime")
                        .HasColumnType("bigint");

                    b.Property<double?>("Last500SpeedRatio")
                        .HasColumnType("float");

                    b.Property<double>("LastFinishTime")
                        .HasColumnType("float");

                    b.Property<double>("LastPlaceFinishTime")
                        .HasColumnType("float");

                    b.Property<long?>("Leader1000HorseId")
                        .HasColumnType("bigint");

                    b.Property<long?>("Leader500HorseId")
                        .HasColumnType("bigint");

                    b.Property<string>("MediaId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("RaceDayId")
                        .HasColumnType("bigint");

                    b.Property<string>("RaceId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("RaceOrder")
                        .HasColumnType("int");

                    b.Property<DateTime>("ScheduledStartTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Sport")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("StartSpeedFigure")
                        .HasColumnType("float");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("StartType")
                        .HasColumnType("int");

                    b.Property<long>("SystemsLost")
                        .HasColumnType("bigint");

                    b.Property<double>("SystemsLostPercent")
                        .HasColumnType("float");

                    b.Property<string>("TrackCondition")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("TrackDayId")
                        .HasColumnType("bigint");

                    b.Property<long?>("WinnerDriverId")
                        .HasColumnType("bigint");

                    b.Property<double>("WinnerFinishTime")
                        .HasColumnType("float");

                    b.Property<long?>("WinnerHorseId")
                        .HasColumnType("bigint");

                    b.Property<double>("WinnerKmTimeMilliseconds")
                        .HasColumnType("float");

                    b.Property<long?>("WinnerTrainerId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("ArenaId");

                    b.HasIndex("Leader1000HorseId");

                    b.HasIndex("Leader500HorseId");

                    b.HasIndex("RaceId")
                        .IsUnique()
                        .HasFilter("[RaceId] IS NOT NULL");

                    b.HasIndex("TrackDayId");

                    b.ToTable("Race");
                });

            modelBuilder.Entity("Travsport.DB.Entities.RaceResult", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool?>("BackChange")
                        .HasColumnType("bit");

                    b.Property<bool?>("BackShoes")
                        .HasColumnType("bit");

                    b.Property<bool>("DQ")
                        .HasColumnType("bit");

                    b.Property<int>("Distance")
                        .HasColumnType("int");

                    b.Property<int>("DistanceHandicap")
                        .HasColumnType("int");

                    b.Property<double>("Distribution")
                        .HasColumnType("float");

                    b.Property<bool?>("DriverChanged")
                        .HasColumnType("bit");

                    b.Property<long?>("DriverId")
                        .HasColumnType("bigint");

                    b.Property<int>("FinishPosition")
                        .HasColumnType("int");

                    b.Property<double>("FinishTimeAfterWinner")
                        .HasColumnType("float");

                    b.Property<double>("FinishTimeMilliseconds")
                        .HasColumnType("float");

                    b.Property<bool?>("FrontChange")
                        .HasColumnType("bit");

                    b.Property<bool?>("FrontShoes")
                        .HasColumnType("bit");

                    b.Property<bool>("Galopp")
                        .HasColumnType("bit");

                    b.Property<long>("HorseId")
                        .HasColumnType("bigint");

                    b.Property<long>("KmTimeMilliSeconds")
                        .HasColumnType("bigint");

                    b.Property<double>("NormalizedFinishTime")
                        .HasColumnType("float");

                    b.Property<double>("NormalizedFinishTimesPlaced")
                        .HasColumnType("float");

                    b.Property<double>("PlatsOdds")
                        .HasColumnType("float");

                    b.Property<int>("PositionForDistance")
                        .HasColumnType("int");

                    b.Property<long>("RaceFKId")
                        .HasColumnType("bigint");

                    b.Property<bool>("Scratched")
                        .HasColumnType("bit");

                    b.Property<double>("SmallDistribution")
                        .HasColumnType("float");

                    b.Property<int>("StartNumber")
                        .HasColumnType("int");

                    b.Property<string>("Sulky")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("TrainerId")
                        .HasColumnType("bigint");

                    b.Property<double>("WinOdds")
                        .HasColumnType("float");

                    b.Property<int>("WonPrizeMoney")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DriverId");

                    b.HasIndex("HorseId");

                    b.HasIndex("RaceFKId");

                    b.HasIndex("TrainerId");

                    b.ToTable("RaceResult");
                });

            modelBuilder.Entity("Travsport.DB.Entities.RecentHorseStart", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("DQ")
                        .HasColumnType("bit");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<long>("Distance")
                        .HasColumnType("bigint");

                    b.Property<bool>("Galloped")
                        .HasColumnType("bit");

                    b.Property<long>("HorseId")
                        .HasColumnType("bigint");

                    b.Property<long>("KmTimeMilliseconds")
                        .HasColumnType("bigint");

                    b.Property<string>("RaceId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Sport")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("StartMethod")
                        .HasColumnType("int");

                    b.Property<int>("Track")
                        .HasColumnType("int");

                    b.Property<double>("WinOdds")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("HorseId");

                    b.HasIndex("RaceId");

                    b.ToTable("RecentHorseStart");
                });

            modelBuilder.Entity("Travsport.DB.Entities.TrackDay", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("ArenaId")
                        .HasColumnType("bigint");

                    b.Property<string>("BetType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<bool>("NewStartList")
                        .HasColumnType("bit");

                    b.Property<int>("NumRaces")
                        .HasColumnType("int");

                    b.Property<bool>("OldStartList")
                        .HasColumnType("bit");

                    b.Property<long>("PrizeMoney")
                        .HasColumnType("bigint");

                    b.Property<string>("RaceDayGameTypes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ShortBetType")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ArenaId");

                    b.ToTable("TrackDay");
                });

            modelBuilder.Entity("Travsport.DB.Entities.TrainerDriver", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<int>("BirthYear")
                        .HasColumnType("int");

                    b.Property<long?>("HomeArenaId")
                        .HasColumnType("bigint");

                    b.Property<bool>("Linkable")
                        .HasColumnType("bit");

                    b.Property<string>("Location")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ShortName")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("HomeArenaId");

                    b.HasIndex("ShortName");

                    b.ToTable("TrainerDriver");
                });

            modelBuilder.Entity("Travsport.DB.Entities.Driver", b =>
                {
                    b.HasOne("Travsport.DB.Entities.Arena", "HomeArena")
                        .WithMany("HomeDrivers")
                        .HasForeignKey("HomeArenaId");
                });

            modelBuilder.Entity("Travsport.DB.Entities.GameDistribution", b =>
                {
                    b.HasOne("Travsport.DB.Entities.ComboGame", "Game")
                        .WithMany("Distributions")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Travsport.DB.Entities.RaceResult", "Result")
                        .WithMany()
                        .HasForeignKey("RaceResultId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Travsport.DB.Entities.GamePayout", b =>
                {
                    b.HasOne("Travsport.DB.Entities.ComboGame", "Game")
                        .WithMany("Payouts")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Travsport.DB.Entities.GameRace", b =>
                {
                    b.HasOne("Travsport.DB.Entities.ComboGame", "Game")
                        .WithMany("Races")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Travsport.DB.Entities.Race", "Race")
                        .WithMany("Races")
                        .HasForeignKey("RaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Travsport.DB.Entities.Horse", b =>
                {
                    b.HasOne("Travsport.DB.Entities.Arena", "HomeArena")
                        .WithMany("HomeHorses")
                        .HasForeignKey("ArenaId");

                    b.HasOne("Travsport.DB.Entities.Breeder", "Breeder")
                        .WithMany("Horses")
                        .HasForeignKey("BreederId");

                    b.HasOne("Travsport.DB.Entities.Horse", "Father")
                        .WithMany()
                        .HasForeignKey("FatherHorseId");

                    b.HasOne("Travsport.DB.Entities.Horse", "GrandFather")
                        .WithMany()
                        .HasForeignKey("GrandFatherHorseId");

                    b.HasOne("Travsport.DB.Entities.Horse", "GrandMother")
                        .WithMany()
                        .HasForeignKey("GrandMotherHorseId");

                    b.HasOne("Travsport.DB.Entities.Horse", "Mother")
                        .WithMany()
                        .HasForeignKey("MotherHorseId");

                    b.HasOne("Travsport.DB.Entities.Owner", "Owner")
                        .WithMany("Horses")
                        .HasForeignKey("OwnerId");

                    b.HasOne("Travsport.DB.Entities.TrainerDriver", "Trainer")
                        .WithMany()
                        .HasForeignKey("TrainerId");
                });

            modelBuilder.Entity("Travsport.DB.Entities.Race", b =>
                {
                    b.HasOne("Travsport.DB.Entities.Arena", "Arena")
                        .WithMany("Races")
                        .HasForeignKey("ArenaId");

                    b.HasOne("Travsport.DB.Entities.Horse", "Leader1000Horse")
                        .WithMany()
                        .HasForeignKey("Leader1000HorseId");

                    b.HasOne("Travsport.DB.Entities.Horse", "Leader500Horse")
                        .WithMany()
                        .HasForeignKey("Leader500HorseId");

                    b.HasOne("Travsport.DB.Entities.TrackDay", null)
                        .WithMany("Races")
                        .HasForeignKey("TrackDayId");
                });

            modelBuilder.Entity("Travsport.DB.Entities.RaceResult", b =>
                {
                    b.HasOne("Travsport.DB.Entities.TrainerDriver", "Driver")
                        .WithMany()
                        .HasForeignKey("DriverId");

                    b.HasOne("Travsport.DB.Entities.Horse", "Horse")
                        .WithMany("RaceResults")
                        .HasForeignKey("HorseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Travsport.DB.Entities.Race", "Race")
                        .WithMany("RaceResults")
                        .HasForeignKey("RaceFKId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Travsport.DB.Entities.TrainerDriver", "Trainer")
                        .WithMany()
                        .HasForeignKey("TrainerId");
                });

            modelBuilder.Entity("Travsport.DB.Entities.RecentHorseStart", b =>
                {
                    b.HasOne("Travsport.DB.Entities.Horse", "Horse")
                        .WithMany()
                        .HasForeignKey("HorseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Travsport.DB.Entities.TrackDay", b =>
                {
                    b.HasOne("Travsport.DB.Entities.Arena", "Arena")
                        .WithMany()
                        .HasForeignKey("ArenaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Travsport.DB.Entities.TrainerDriver", b =>
                {
                    b.HasOne("Travsport.DB.Entities.Arena", "HomeArena")
                        .WithMany()
                        .HasForeignKey("HomeArenaId");
                });
#pragma warning restore 612, 618
        }
    }
}
