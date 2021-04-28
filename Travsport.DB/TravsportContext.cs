using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Travsport.DB.Entities;

namespace Travsport.DB
{

    public class TravsportContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Arena>().Property(a => a.Id).ValueGeneratedOnAdd();
            mb.Entity<ComboGame>().Property(a => a.Id).ValueGeneratedOnAdd();
            //mb.Entity<Race>().Property(a => a.Id).ValueGeneratedOnAdd();
            mb.Entity<RaceResult>().Property(a => a.Id).ValueGeneratedOnAdd();

            mb.Entity<ComboGame>().HasMany(t => t.Races);
            mb.Entity<ComboGame>().HasMany(t => t.Payouts).WithOne(p => p.Game);
            mb.Entity<Arena>().HasMany(t => t.Races).WithOne(m => m.Arena);
            mb.Entity<Arena>().HasMany(t => t.HomeHorses).WithOne(m => m.HomeArena);

            mb.Entity<Race>().HasMany(t => t.Races);

            mb.Entity<Race>().HasMany(t => t.RaceResults).WithOne(m => m.Race);

            mb.Entity<Owner>().HasMany(o => o.Horses).WithOne(h => h.Owner);

            mb.Entity<Breeder>().HasMany(o => o.Horses).WithOne(h => h.Breeder);


            mb.Entity<Horse>().HasOne(o => o.Father).WithMany();
            mb.Entity<Horse>().HasOne(o => o.Mother).WithMany();

            mb.Entity<Horse>().HasOne(o => o.GrandFather).WithMany();
            mb.Entity<Horse>().HasOne(o => o.GrandMother).WithMany();
            mb.Entity<TrainerDriver>().HasIndex(t => t.ShortName).IsUnique(false);


            mb.Entity<Horse>().HasIndex(h => h.Name).IsUnique();


            mb.Entity<RecentHorseStart>().HasIndex(h => h.RaceId);
            mb.Entity<RecentHorseStart>().HasIndex(h => h.HorseId);

            mb.Entity<Race>().HasIndex(r => r.RaceId).IsUnique();

            mb.Entity<AvailableGame>().HasIndex(ag => ag.GameId).IsUnique(true);

            mb.Entity<RaceResult>().HasOne(r => r.HorseStats);

            mb.Entity<HorseStats>().HasOne(r => r.KmTimeValidProfile);
            mb.Entity<HorseStats>().HasOne(r => r.TimeAfterWinnerLastCapProfile);
            mb.Entity<HorseStats>().HasOne(r => r.TimeAfterWinnerMinMaxProfile);
            mb.Entity<HorseStats>().HasOne(r => r.TimeAfterWinnerNormMinPlacedCapProfile);
            mb.Entity<HorseStats>().HasOne(r => r.TimeAfterWinnerPlaceCapProfile);
      

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=localhost\\sqlexpress;Initial Catalog=travsport2;Integrated Security=True", options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });


            base.OnConfiguring(optionsBuilder);

        }

        public DbSet<Arena> Arenas { get; set; }
        public DbSet<HorseStats> HorseStats { get; set; }
        public DbSet<ComboGame> ComboGames { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Horse> Horses { get; set; }
        public DbSet<Race> Races { get; set; }
        public DbSet<RaceResult> RaceResults { get; set; }

        // public DbSet<HorseStats> HorseStats { get; set; }
        public DbSet<TrainerDriver> TrainerDrivers { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Breeder> Breeders { get; set; }
        public DbSet<RecentHorseStart> RecentHorseStarts { get; set; }
        public DbSet<AvailableGame> AvailableGames { get; set; }
        public DbSet<TrackDay> TrackDays { get; set; }
        public Dictionary<long, double> GetStdDev(IEnumerable<long> horseIds)
        {
            var all = RaceResults.Where(hs => horseIds.Contains(hs.Horse.Id));
            all = all.Where(hs => hs.KmTimeMilliSeconds > 0);
            return all.GroupBy(hs => hs.Horse.Id).ToDictionary(hs => hs.Key, g => StdDev(g.Select(hs => hs.KmTimeMilliSeconds)));

        }
        public static double StdDev(IEnumerable<long> numbs)
        {
            if (!numbs.Any())
                return 0;
            var avg = numbs.Average();
            return Math.Sqrt(numbs.Sum(n => Math.Pow(n - avg, 2)) / numbs.Count());
        }
        public TrackDay AssureTrackDay(TrackDay trackDay, long arenaId)
        {
            var exists = TrackDays.SingleOrDefault(td => td.Date == trackDay.Date && td.Arena.Id == arenaId);
            if (exists != null)
            {
                return exists;
            }
            TrackDays.Add(trackDay);
            return trackDay;
        }

        public AvailableGame AddAvailableGame(string gameId, DateTime date)
        {
            var game = new AvailableGame();
            game.GameId = gameId;
            game.ScheduledStartTime = date;
            AvailableGames.Add(game);
            return game;
        }
        public AvailableGame AssureAvailableGame(string gameId, DateTime date)
        {
            var exists = AvailableGames.SingleOrDefault(ag => ag.GameId == gameId);
            if (exists != null)
            {
                return exists;
            }
            var game = new AvailableGame();
            game.GameId = gameId;
            game.ScheduledStartTime = date;
            AvailableGames.Add(game);
            return game;
        }
    }
}
