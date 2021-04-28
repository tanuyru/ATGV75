﻿// <auto-generated />
using System;
using ATG.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ATG.DB.Migrations
{
    [DbContext(typeof(AtgContext))]
    [Migration("20200917124745_AddedMoreData")]
    partial class AddedMoreData
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ATG.DB.Entities.Arena", b =>
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

            modelBuilder.Entity("ATG.DB.Entities.AvailableGame", b =>
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

            modelBuilder.Entity("ATG.DB.Entities.Breeder", b =>
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

            modelBuilder.Entity("ATG.DB.Entities.ComboGame", b =>
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

            modelBuilder.Entity("ATG.DB.Entities.Driver", b =>
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
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("HomeArenaId");

                    b.HasIndex("ShortName");

                    b.ToTable("Driver");
                });

            modelBuilder.Entity("ATG.DB.Entities.GameDistribution", b =>
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

            modelBuilder.Entity("ATG.DB.Entities.GamePayout", b =>
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

            modelBuilder.Entity("ATG.DB.Entities.GameRace", b =>
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

            modelBuilder.Entity("ATG.DB.Entities.Horse", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<int>("BirthYear")
                        .HasColumnType("int");

                    b.Property<long?>("BreederId")
                        .HasColumnType("bigint");

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

                    b.Property<long?>("HomeArenaId")
                        .HasColumnType("bigint");

                    b.Property<double>("Money")
                        .HasColumnType("float");

                    b.Property<long?>("MotherHorseId")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<long?>("OwnerId")
                        .HasColumnType("bigint");

                    b.Property<long>("StartPoints")
                        .HasColumnType("bigint");

                    b.Property<long?>("TrainerId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("BreederId");

                    b.HasIndex("FatherHorseId");

                    b.HasIndex("GrandFatherHorseId");

                    b.HasIndex("GrandMotherHorseId");

                    b.HasIndex("HomeArenaId");

                    b.HasIndex("MotherHorseId");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasFilter("[Name] IS NOT NULL");

                    b.HasIndex("OwnerId");

                    b.HasIndex("TrainerId");

                    b.ToTable("Horse");
                });

            modelBuilder.Entity("ATG.DB.Entities.Owner", b =>
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

            modelBuilder.Entity("ATG.DB.Entities.Race", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long?>("ArenaId")
                        .HasColumnType("bigint");

                    b.Property<int>("Distance")
                        .HasColumnType("int");

                    b.Property<long?>("First1000KmTime")
                        .HasColumnType("bigint");

                    b.Property<long?>("First500KmTime")
                        .HasColumnType("bigint");

                    b.Property<long?>("Last500KmTime")
                        .HasColumnType("bigint");

                    b.Property<int?>("Leader1000StartNumber")
                        .HasColumnType("int");

                    b.Property<int?>("Leader500StartNumber")
                        .HasColumnType("int");

                    b.Property<string>("MediaId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RaceId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("RaceOrder")
                        .HasColumnType("int");

                    b.Property<DateTime>("ScheduledStartTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Sport")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("StartType")
                        .HasColumnType("int");

                    b.Property<long>("SystemsLost")
                        .HasColumnType("bigint");

                    b.Property<double>("SystemsLostPercent")
                        .HasColumnType("float");

                    b.Property<double>("WinnerFinishTime")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("ArenaId");

                    b.HasIndex("RaceId")
                        .IsUnique()
                        .HasFilter("[RaceId] IS NOT NULL");

                    b.ToTable("Race");
                });

            modelBuilder.Entity("ATG.DB.Entities.RaceResult", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<double>("AverageOddsLastFive")
                        .HasColumnType("float");

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

                    b.Property<long>("DriverId")
                        .HasColumnType("bigint");

                    b.Property<double>("DriverMoney")
                        .HasColumnType("float");

                    b.Property<double>("DriverMoneyLastYear")
                        .HasColumnType("float");

                    b.Property<int>("DriverSeconds")
                        .HasColumnType("int");

                    b.Property<int>("DriverStarts")
                        .HasColumnType("int");

                    b.Property<int>("DriverThirds")
                        .HasColumnType("int");

                    b.Property<double>("DriverWinPercent")
                        .HasColumnType("float");

                    b.Property<int>("DriverWins")
                        .HasColumnType("int");

                    b.Property<bool>("EstimatedFinishTime")
                        .HasColumnType("bit");

                    b.Property<int>("FinishPosition")
                        .HasColumnType("int");

                    b.Property<float>("FinishTimeAfterWinner")
                        .HasColumnType("real");

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

                    b.Property<double>("HorseMoneyLastYear")
                        .HasColumnType("float");

                    b.Property<double>("HorseMoneyThisYear")
                        .HasColumnType("float");

                    b.Property<double>("HorseMoneyTotal")
                        .HasColumnType("float");

                    b.Property<double>("HorseTotalMoney")
                        .HasColumnType("float");

                    b.Property<double>("HorseTotalPlacepercent")
                        .HasColumnType("float");

                    b.Property<double>("HorseTotalWinPercent")
                        .HasColumnType("float");

                    b.Property<double>("HorseWinPercent")
                        .HasColumnType("float");

                    b.Property<TimeSpan>("KmTime")
                        .HasColumnType("time");

                    b.Property<long>("KmTimeMilliSeconds")
                        .HasColumnType("bigint");

                    b.Property<int>("LastYearDriverSeconds")
                        .HasColumnType("int");

                    b.Property<int>("LastYearDriverStarts")
                        .HasColumnType("int");

                    b.Property<int>("LastYearDriverThirds")
                        .HasColumnType("int");

                    b.Property<double>("LastYearDriverWinPercent")
                        .HasColumnType("float");

                    b.Property<int>("LastYearDriverWins")
                        .HasColumnType("int");

                    b.Property<double>("LastYearHorseWinPercent")
                        .HasColumnType("float");

                    b.Property<double>("LastYearMoneyPerStart")
                        .HasColumnType("float");

                    b.Property<int>("LastYearSeconds")
                        .HasColumnType("int");

                    b.Property<int>("LastYearStarts")
                        .HasColumnType("int");

                    b.Property<int>("LastYearThirds")
                        .HasColumnType("int");

                    b.Property<double>("LastYearTrainerSeconds")
                        .HasColumnType("float");

                    b.Property<double>("LastYearTrainerStarts")
                        .HasColumnType("float");

                    b.Property<double>("LastYearTrainerThirds")
                        .HasColumnType("float");

                    b.Property<double>("LastYearTrainerWinPercent")
                        .HasColumnType("float");

                    b.Property<double>("LastYearTrainerWins")
                        .HasColumnType("float");

                    b.Property<int>("LastYearWins")
                        .HasColumnType("int");

                    b.Property<double>("MoneyPerStart")
                        .HasColumnType("float");

                    b.Property<double>("MoneyPerStartTotal")
                        .HasColumnType("float");

                    b.Property<double>("PlatsOdds")
                        .HasColumnType("float");

                    b.Property<int>("Position")
                        .HasColumnType("int");

                    b.Property<int>("PrizeMoney")
                        .HasColumnType("int");

                    b.Property<long>("RaceFKId")
                        .HasColumnType("bigint");

                    b.Property<bool>("Scratched")
                        .HasColumnType("bit");

                    b.Property<int>("Seconds")
                        .HasColumnType("int");

                    b.Property<int>("Starts")
                        .HasColumnType("int");

                    b.Property<int>("Thirds")
                        .HasColumnType("int");

                    b.Property<long>("TimeBehindWinner")
                        .HasColumnType("bigint");

                    b.Property<int>("Track")
                        .HasColumnType("int");

                    b.Property<long?>("TrainerId")
                        .HasColumnType("bigint");

                    b.Property<double>("TrainerMoneyLastYear")
                        .HasColumnType("float");

                    b.Property<double>("TrainerMoneyThisYear")
                        .HasColumnType("float");

                    b.Property<double>("TrainerSeconds")
                        .HasColumnType("float");

                    b.Property<double>("TrainerStarts")
                        .HasColumnType("float");

                    b.Property<double>("TrainerThirds")
                        .HasColumnType("float");

                    b.Property<double>("TrainerWinPercent")
                        .HasColumnType("float");

                    b.Property<double>("TrainerWins")
                        .HasColumnType("float");

                    b.Property<double>("WinOdds")
                        .HasColumnType("float");

                    b.Property<int>("Wins")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DriverId");

                    b.HasIndex("HorseId");

                    b.HasIndex("RaceFKId");

                    b.HasIndex("TrainerId");

                    b.ToTable("RaceResult");
                });

            modelBuilder.Entity("ATG.DB.Entities.RecentHorseStart", b =>
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

            modelBuilder.Entity("ATG.DB.Entities.TrackDay", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("ArenaId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<int>("NumRaces")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ArenaId");

                    b.ToTable("TrackDay");
                });

            modelBuilder.Entity("ATG.DB.Entities.Trainer", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<string>("Location")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ShortName")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("ShortName");

                    b.ToTable("Trainer");
                });

            modelBuilder.Entity("ATG.DB.Entities.Driver", b =>
                {
                    b.HasOne("ATG.DB.Entities.Arena", "HomeArena")
                        .WithMany("HomeDrivers")
                        .HasForeignKey("HomeArenaId");
                });

            modelBuilder.Entity("ATG.DB.Entities.GameDistribution", b =>
                {
                    b.HasOne("ATG.DB.Entities.ComboGame", "Game")
                        .WithMany("Distributions")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ATG.DB.Entities.RaceResult", "Result")
                        .WithMany("Distributions")
                        .HasForeignKey("RaceResultId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ATG.DB.Entities.GamePayout", b =>
                {
                    b.HasOne("ATG.DB.Entities.ComboGame", "Game")
                        .WithMany("Payouts")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ATG.DB.Entities.GameRace", b =>
                {
                    b.HasOne("ATG.DB.Entities.ComboGame", "Game")
                        .WithMany("Races")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ATG.DB.Entities.Race", "Race")
                        .WithMany("Races")
                        .HasForeignKey("RaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ATG.DB.Entities.Horse", b =>
                {
                    b.HasOne("ATG.DB.Entities.Breeder", "Breeder")
                        .WithMany("Horses")
                        .HasForeignKey("BreederId");

                    b.HasOne("ATG.DB.Entities.Horse", "Father")
                        .WithMany()
                        .HasForeignKey("FatherHorseId");

                    b.HasOne("ATG.DB.Entities.Horse", "GrandFather")
                        .WithMany()
                        .HasForeignKey("GrandFatherHorseId");

                    b.HasOne("ATG.DB.Entities.Horse", "GrandMother")
                        .WithMany()
                        .HasForeignKey("GrandMotherHorseId");

                    b.HasOne("ATG.DB.Entities.Arena", "HomeArena")
                        .WithMany("HomeHorses")
                        .HasForeignKey("HomeArenaId");

                    b.HasOne("ATG.DB.Entities.Horse", "Mother")
                        .WithMany()
                        .HasForeignKey("MotherHorseId");

                    b.HasOne("ATG.DB.Entities.Owner", "Owner")
                        .WithMany("Horses")
                        .HasForeignKey("OwnerId");

                    b.HasOne("ATG.DB.Entities.Trainer", "Trainer")
                        .WithMany("Horses")
                        .HasForeignKey("TrainerId");
                });

            modelBuilder.Entity("ATG.DB.Entities.Race", b =>
                {
                    b.HasOne("ATG.DB.Entities.Arena", "Arena")
                        .WithMany("Races")
                        .HasForeignKey("ArenaId");
                });

            modelBuilder.Entity("ATG.DB.Entities.RaceResult", b =>
                {
                    b.HasOne("ATG.DB.Entities.Driver", "Driver")
                        .WithMany()
                        .HasForeignKey("DriverId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ATG.DB.Entities.Horse", "Horse")
                        .WithMany("RaceResults")
                        .HasForeignKey("HorseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ATG.DB.Entities.Race", "Race")
                        .WithMany("RaceResults")
                        .HasForeignKey("RaceFKId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ATG.DB.Entities.Trainer", "Trainer")
                        .WithMany()
                        .HasForeignKey("TrainerId");
                });

            modelBuilder.Entity("ATG.DB.Entities.RecentHorseStart", b =>
                {
                    b.HasOne("ATG.DB.Entities.Horse", "Horse")
                        .WithMany()
                        .HasForeignKey("HorseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ATG.DB.Entities.TrackDay", b =>
                {
                    b.HasOne("ATG.DB.Entities.Arena", "Arena")
                        .WithMany()
                        .HasForeignKey("ArenaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
