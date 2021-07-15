using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class MasterBotContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<AutoRole> AutoRoles { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<LFG> LFGs { get; set; }
        public DbSet<User> Users { get; set; }
        
        
        
        protected override void OnConfiguring(DbContextOptionsBuilder options) 
            => options.UseMySql("server=localhost;user=root;database=masterbot;port=3306;Connect Timeout=5;", 
            // new MySqlServerVersion(new Version(8, 0, 25))); // new Version(8, 0, 21) https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/pull/1233
            new MariaDbServerVersion(new Version(10, 4, 19)));
    }   
    public class Server
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }

        public List<LFG> LFGs { get; set; }
        public ulong AdminChannel { get; set; }
        public ulong LogChannel { get; set; }

        public ulong ActivityMessage { get; set; }
        public bool LoggingOn { get; set; }
        public int InactivityPeriod { get; set; }
        // public int GracePeriod { get; set; }
    }


    public class User
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public DateTime LastActivity { get; set; }
        public ulong GuildId { get; set; }
    }
    public class Rank
    {
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }        
    }

    public class AutoRole
    {
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }        
    }

    public class Game
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }   
        public string ShortName { get; set; }
        public bool IntegerRankingSystem { get; set; }
    }

    public class Player
    {
        [Key]
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public ulong ServerId { get; set; }
        public Game Game { get; set; }
        public Region Region { get; set; }
        public Rating Rank { get; set; }
    }

    public class LFG
    {
        [Key]
        public int Id { get; set; }
        public ulong ServerId { get; set; }
        public Player Player { get; set; }
        public Game Game { get; set; }
        public Mode Mode { get; set; }

        public Rating LowerRatingBound { get; set; }
        public Rating UpperRatingBound { get; set; }
    }
    public enum Mode
    {
        Casual,
        Ranked,
        Coop,
        Customs
    }
    public enum Region
    {
        EU,
        NA,
        Africa,
        SEA,
        Asia,
        ME,
        LA,
        RU
    }

    public enum Rating
    {
        None,

        Iron,
        Copper,

        //sc2
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        
        // COD:
        Competitor,
        Advanced,
        Expert,
        Elite,
        Master,

        // DOTA2:
        Herald1,
        Herald2,
        Herald3,
        Herald4,
        Herald5,
        
        Guardian1,
        Guardian2,
        Guardian3,
        Guardian4,
        Guardian5,

        Crusader1,
        Crusader2,
        Crusader3,
        Crusader4,
        Crusader5,
        Archon1,
        Archon2,
        Archon3,
        Archon4,
        Archon5,

        Legend1,
        Legend2,
        Legend3,
        Legend4,
        Legend5,

        Ancient1,
        Ancient2,
        Ancient3,
        Ancient4,
        Ancient5,

        Divine1,
        Divine2,
        Divine3,
        Divine4,
        Divine5,

        // CSGO:
        Silver1,
        Silver2,
        Silver3,
        Silver4,
        SilverElite,
        SilverEliteMaster,
        GoldNova1,
        GoldNova2,
        GoldNova3,
        GoldNovaMaster,
        MasterGuardian1,
        MasterGuardian2,
        MasterGuardianElite,
        DistinguishedMasterGuardian,
        LegendaryEagle,
        LegendaryEagleMaster,
        SupremeMasterFirstClass,
        GlobalElite,

        GrandMaster,
        Challenger,
        Immortal,
        Radiant,
        Champions,
        Onyx,
        Champion



    }
}
