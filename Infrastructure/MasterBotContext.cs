using System;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class MasterBotContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<AutoRole> AutoRoles { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder options) 
            => options.UseMySql("server=localhost;user=root;database=masterbot;port=3306;", new MariaDbServerVersion(new Version(10, 4, 19))); // new Version(8, 0, 21) https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/pull/1233
    }
    public class Server
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }
        
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
}
