using ATG.DB.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace ATG.DB
{
    public class AtgContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Arena>().Property(a => a.Id).ValueGeneratedOnAdd();
            mb.Entity<ComboGame>().Property(a => a.Id).ValueGeneratedOnAdd();
            mb.Entity<Race>().Property(a => a.Id).ValueGeneratedOnAdd();
            mb.Entity<RaceResult>().Property(a => a.Id).ValueGeneratedOnAdd();

            mb.Entity<ComboGame>().HasMany(t => t.Races);
            mb.Entity<ComboGame>().HasMany(t => t.Payouts).WithOne(p => p.Game);
            mb.Entity<Arena>().HasMany(t => t.Races).WithOne(m => m.Arena);
            mb.Entity<Arena>().HasMany(t => t.HomeHorses).WithOne(m => m.HomeArena);

            mb.Entity<Race>().HasMany(t => t.Races);

            mb.Entity<Race>().HasMany(t => t.RaceResults).WithOne(m => m.Race);
        
            mb.Entity<Owner>().HasMany(o => o.Horses).WithOne(h => h.Owner);
            mb.Entity<Trainer>().HasMany(o => o.Horses).WithOne(h => h.Trainer);
            mb.Entity<Breeder>().HasMany(o => o.Horses).WithOne(h => h.Breeder);

            
            mb.Entity<Horse>().HasOne(o => o.Father).WithMany();
            mb.Entity<Horse>().HasOne(o => o.Mother).WithMany();

            mb.Entity<Horse>().HasOne(o => o.GrandFather).WithMany();
            mb.Entity<Horse>().HasOne(o => o.GrandMother).WithMany();
            mb.Entity<Trainer>().HasIndex(t => t.ShortName).IsUnique(false);
            mb.Entity<Driver>().HasIndex(t => t.ShortName).IsUnique(false);

            mb.Entity<Horse>().HasIndex(h => h.Name).IsUnique(false);
            mb.Entity<Driver>().HasIndex(d => d.ShortName).IsUnique(false);

            mb.Entity<RecentHorseStart>().HasIndex(h => h.RaceId); 
            mb.Entity<RecentHorseStart>().HasIndex(h => h.HorseId);
            
            mb.Entity<Race>().HasIndex(r => r.RaceId).IsUnique();

            mb.Entity<AvailableGame>().HasIndex(ag => ag.GameId).IsUnique(true);
            /*
            mb
           .Entity<HorseStats>(eb =>
           {
               eb.HasNoKey();
               eb.ToView("HorseStats");
               eb.Property(v => v.Name).HasColumnName("Name");
               eb.Property(v => v.HorseId).HasColumnName("HorseId");
           });*/
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=localhost\\sqlexpress;Initial Catalog=atg2;Integrated Security=True", options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });


            base.OnConfiguring(optionsBuilder);
  
        }

        public DbSet<Arena> Arenas { get; set; }
        public DbSet<ComboGame> ComboGames { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Horse> Horses { get; set; }
        public DbSet<Race> Races { get; set; }
        public DbSet<RaceResult> RaceResults { get; set; }
        public DbSet<GameDistribution> GameDistributions { get; set; }

       // public DbSet<HorseStats> HorseStats { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
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
        public Trainer AssureTrainer(Trainer breeder)
        {
            if (breeder.Id == 0)
            {
                var namedTrainer = Trainers.FirstOrDefault(db => db.Name == breeder.Name) ?? Trainers.Local.FirstOrDefault(db => db.Name == breeder.Name);
                if (namedTrainer != null)
                    return namedTrainer;
                breeder.Id = GetNextTrainerId();
            }
            var t = Trainers.FirstOrDefault(db => db.Id == breeder.Id) ?? Trainers.Local.FirstOrDefault(db => db.Id == breeder.Id);
            if (t != null)
            {
                return t;
            }
            Trainers.Add(breeder);
            return breeder;
        }
        private long GetNextTrainerId()
        {
            var minId = Math.Min(Trainers.Any() ? Trainers.Min(h => h.Id) : 1, Trainers.Local.Any() ? Breeders.Local.Min(h => h.Id) : 1);
            if (minId > 0)
                minId = -1;
            return minId - 1;
        }
        public Owner AssureOwner(Owner owner)
        {
            if (owner.Id == 0)
            {
                var namedTrainer = Owners.FirstOrDefault(db => db.Name == owner.Name) ?? Owners.Local.FirstOrDefault(db => db.Name == owner.Name);
                if (namedTrainer != null)
                    return namedTrainer;
                owner.Id = GetNextOwnerId();
            }
            var t = Owners.FirstOrDefault(db => db.Id == owner.Id) ?? Owners.Local.FirstOrDefault(db => db.Id == owner.Id);
            if (t != null)
            {
                return t;
            }
            Owners.Add(owner);
            return owner;
        }
        private long GetNextOwnerId()
        {
            var minId = Math.Min(Breeders.Any() ? Breeders.Min(h => h.Id) : 1, Breeders.Local.Any() ? Breeders.Local.Min(h => h.Id) : 1);
            if (minId > 0)
                minId = -1;
            return minId - 1;
        }

        public Breeder AssureBreeder(Breeder breeder)
        {
            if (breeder.Id == 0)
            {
                var namedTrainer = Breeders.FirstOrDefault(db => db.Name == breeder.Name) ?? Breeders.Local.FirstOrDefault(db => db.Name == breeder.Name);
                if (namedTrainer != null)
                    return namedTrainer;
                breeder.Id = GetNextBreederId();
            }
            var t = Breeders.FirstOrDefault(db => db.Id == breeder.Id) ?? Breeders.Local.FirstOrDefault(db => db.Id == breeder.Id);
            if (t != null)
            {
                return t;
            }
            Breeders.Add(breeder);
            return breeder;
        }
        private long GetNextBreederId()
        {
            var minId = Math.Min(Breeders.Any() ? Breeders.Min(h => h.Id) : 1, Breeders.Local.Any() ? Breeders.Local.Min(h => h.Id) : 1);
            if (minId > 0)
                minId = -1;
            return minId - 1;
        }
        private long GetNextArenaId()
        {
            var minId = Math.Min(Arenas.Any() ? Arenas.Min(h => h.Id) : 1, Arenas.Local.Any() ? Arenas.Local.Min(h => h.Id) : 1);
            if (minId > 0)
                minId = -1;
            return minId - 1;
        }
        public long AddArena(Arena a)
        {
            if (a.Id == 0)
            {
                var existing = Arenas.FirstOrDefault(arena => arena.Name == a.Name);
                if (existing == null)
                {
                    a.Id = GetNextArenaId();
                }
                else
                {
                    return existing.Id;
                }
            }
            Arenas.Add(a);
            return a.Id;
        }
        public Arena AssureArena(Arena a)
        {
            if (a.Id == 0)
            {
                var existingName = Arenas.FirstOrDefault(arena => arena.Name == a.Name) ?? Arenas.Local.FirstOrDefault(arena => arena.Name == a.Name);
                if (existingName != null)
                {
                    if (existingName.Country == null)
                    {
                        existingName.Country = a.Country;
                    }
                    return existingName;
                }
                a.Id = GetNextArenaId();
            }
            var existing = Arenas.FirstOrDefault(arena => arena.Id == a.Id) ?? Arenas.Local.FirstOrDefault(arena => arena.Id == a.Id);
            if (existing != null)
            {
                if (existing.Country == null && a.Country != null)
                    existing.Country = a.Country;
                if (existing.Condition == null && a.Condition != null)
                    existing.Condition = a.Condition;
                return existing;
            }
            Arenas.Add(a);
            return a;
        }
        private HashSet<long> usedHorseIds = new HashSet<long>();
        private long GetNextHorseId()
        {
            var minId = Math.Min(Horses.Any() ? Horses.Min(h => h.Id) : 1, Horses.Local.Any() ? Horses.Local.Min(h => h.Id) : 1);
            if (minId > 0)
                minId = -1;
            minId--;
            while (!usedHorseIds.Add(minId))
                minId--;
            return minId;
        }
        public void AddRecentStart(RecentHorseStart rs)
        {
            RecentHorseStarts.Add(rs);
        }
        public RecentHorseStart AssureRecentStart(RecentHorseStart rs)
        {
            var exist = RecentHorseStarts.SingleOrDefault(rhs => rs.RaceId == rhs.RaceId && rhs.Horse.Id == rs.Horse.Id)
                ?? RecentHorseStarts.Local.SingleOrDefault(rhs => rs.RaceId == rhs.RaceId && rhs.Horse.Id == rs.Horse.Id);
            if (exist != null)
            {
                if (exist.RaceId == null)
                    exist.RaceId = rs.RaceId;
                if (exist.Distance == 0)
                    exist.Distance = rs.Distance;
                if (exist.StartMethod == Shared.Enums.StartTypeEnum.Unknown)
                    exist.StartMethod = rs.StartMethod;
                return exist;
            }
            RecentHorseStarts.Add(rs);
            return rs;
        }
        public Horse AddHorse(Horse a)
        {
            if (a.Id == 0)
            {
                var existingNamed = Horses.FirstOrDefault(arena => arena.Name == a.Name) ?? Horses.Local.FirstOrDefault(h => h.Name == a.Name);
                if (existingNamed == null)
                {
                    a.Id = GetNextHorseId();
                }
                else
                {
                    if (existingNamed.HomeArena == null && a.HomeArena != null)
                        existingNamed.HomeArena = a.HomeArena;
                    if (existingNamed.Gender == null)
                        existingNamed.Gender = a.Gender;
                    return existingNamed;
                }
            }
            Horses.Add(a);
            return a;
        }
        public Horse AssureHorse(Horse a)
        {
            if (a.Id == 0)
            {
                var existingNamed = Horses.FirstOrDefault(arena => arena.Name == a.Name) ?? Horses.Local.FirstOrDefault(h => h.Name == a.Name);
                if (existingNamed == null)
                {
                    a.Id = GetNextHorseId();
                }
                else
                {
                    if (existingNamed.HomeArena == null && a.HomeArena != null)
                        existingNamed.HomeArena = a.HomeArena;
                    if (existingNamed.Gender == null)
                        existingNamed.Gender = a.Gender;
                    return existingNamed;
                }
            }
            else
            {
                var existing = Horses.FirstOrDefault(arena => arena.Id == a.Id);
                if (existing == null)
                {
                    existing = Horses.Local.FirstOrDefault(h => h.Id == a.Id);
                    if (existing != null)
                    {
                        if (existing.HomeArena == null && a.HomeArena != null)
                            existing.HomeArena = a.HomeArena;
                        if (existing.Gender == null)
                            existing.Gender = a.Gender;
                        return existing;
                    }
                }
                else
                {
                    if (existing.HomeArena == null && a.HomeArena != null)
                        existing.HomeArena = a.HomeArena;
                    if (existing.Gender == null)
                        existing.Gender = a.Gender;
                    return existing;
                }
                var existingNamed = Horses.FirstOrDefault(arena => arena.Name == a.Name) ?? Horses.Local.FirstOrDefault(h => h.Name == a.Name);
                if (existingNamed != null)
                {
                    if (existingNamed.HomeArena == null && a.HomeArena != null)
                        existingNamed.HomeArena = a.HomeArena;
                    if (existingNamed.Gender == null)
                        existingNamed.Gender = a.Gender;
                    return existingNamed;
                }
            }
            
            if (!Horses.Any(h => h.Id == a.Id) && !Horses.Local.Any(h => h.Id == a.Id))
                Horses.Add(a);

            return a;
        }
        public Race AssureRace(Race a)
        {
            var existing = Races.Include(r => r.Races).FirstOrDefault(arena => arena.RaceId == a.RaceId) ?? Races.Local.FirstOrDefault(arena => arena.RaceId == a.RaceId);
            if (existing != null)
            {
                if (existing.Sport == null)
                {
                    existing.Sport = a.Sport;
                }
                return existing;
            }
            Races.Add(a);
            return a;
        
        }
      
        public ComboGame AssureGame(ComboGame a)
        {
            var existing = ComboGames.Include(r => r.Races).ThenInclude(gr => gr.Race).FirstOrDefault(arena => arena.GameId == a.GameId) ?? ComboGames.Local.FirstOrDefault(arena => arena.GameId == a.GameId);
            if (existing != null)
                return existing;
            ComboGames.Add(a);
            return a;
        }
        private long GetNextDriverId()
        {
            var min = Math.Min(Drivers.Any() ? Drivers.Min(d => d.Id) : 1, Drivers.Local.Any() ? Drivers.Local.Min(d => d.Id) : 1);
            if (min > 0)
                min = -1;
            return min - 1;
        }

        public Driver AssureDriver(Driver a)
        {
            if (a.Id == 0)
            {
                var existingNamed = Drivers.FirstOrDefault(arena => arena.Name == a.Name) ?? Drivers.Local.FirstOrDefault(arena => arena.Name == a.Name);
                if (existingNamed == null)
                {
                    a.Id = GetNextDriverId();
                }
                else
                {
                    if (existingNamed.HomeArena == null && a.HomeArena != null)
                    {
                        existingNamed.HomeArena = a.HomeArena;
                    }
                    return existingNamed;
                }
            }
            var existing = Drivers.FirstOrDefault(arena => arena.Id == a.Id) ?? Drivers.Local.FirstOrDefault(arena => arena.Id == a.Id);
            if (existing != null)
            {
                if (existing.HomeArena == null && a.HomeArena != null)
                {
                    existing.HomeArena = a.HomeArena;
                }
                return existing;
            }
            
            Drivers.Add(a);
            return a;
        }
        public void AddRaceResult(RaceResult a)
        {
            RaceResults.Add(a);
        }

        public RaceResult AssureRaceResult(RaceResult a)
        {
            var existing = RaceResults.Include(rr => rr.Race).FirstOrDefault(arena => arena.RaceFKId == a.RaceFKId && arena.HorseId == a.HorseId) ?? RaceResults.Local.FirstOrDefault(arena => arena.RaceFKId == a.RaceFKId && arena.HorseId == a.HorseId);
            if (existing != null)
            {
                if (existing.FinishPosition == 0)
                {
                    existing.FinishPosition = a.FinishPosition;
                }
                return existing;
            }
            RaceResults.Add(a);
            return a;
        }

        public GameDistribution AssureDistribution(GameDistribution dist)
        {
            var existing = GameDistributions
                .FirstOrDefault(d => d.Game.GameId == dist.Game.GameId && d.Result.Id == dist.Result.Id) ??
                GameDistributions.Local.FirstOrDefault(d => d.Game.GameId == dist.Game.GameId && d.Result.Id == dist.Result.Id);
            if (existing != null)
            {
                if (existing.SystemsLeft == 0)
                {
                    existing.SystemsLeft = dist.SystemsLeft;
                }
                if (dist.Distribution != 0)
                {
                    existing.Distribution = dist.Distribution;
                }
                return existing;
            }
            GameDistributions.Add(dist);
            return dist;
        }
    }
}
