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
        
        protected override void OnConfiguring(DbContextOptionsBuilder options) 
            => options.UseMySql("server=localhost;user=root;database=masterbot;port=3306;Connect Timeout=5;", new MariaDbServerVersion(new Version(10,4,19))); // new Version(8, 0, 21) https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/pull/1233
    }
    public class Server
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }

        public List<LFG> LFGs { get; set; }
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
        Competitor,
        Advanced,
        Expert,
        Elite,
        Master
    }
}
