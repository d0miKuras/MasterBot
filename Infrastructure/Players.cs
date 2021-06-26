using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class Players
    {
        private readonly MasterBotContext _context;

        public Players(MasterBotContext context)
        {
            _context = context;
        }

        public async Task<List<Player>> GetPlayersAsync(ulong serverId)
        {
            var players = await _context.Players
                .Where(x => x.ServerId == serverId)
                .ToListAsync();
            
            return await Task.FromResult(players);
        }

        public async Task<Player> GetPlayerAsync(ulong serverId, ulong userId)
        {
                var player = await _context.Players
                    .FirstOrDefaultAsync(x => (x.ServerId == serverId && x.UserId == userId));
            
            return await Task.FromResult(player);
        }

        public async Task AddPlayerAsync(ulong userId, ulong serverId, string gameName, string rank, Region region)
        {
            var server = await _context.Servers
                .FindAsync(serverId);
            
            if(server == null)
                _context.Add(new Server {Id = serverId});

            var game = await _context.Games
                .FirstOrDefaultAsync(x => (string.Equals(x.Name, gameName) || string.Equals(x.ShortName, gameName)));
            
            var player = await _context.Players
                .FirstOrDefaultAsync(x => (x.UserId == userId && x.ServerId == serverId && x.Region == region && x.Game == game));

            if(game != null && player == null)
            {
                await _context.AddAsync(new Player {ServerId = serverId, UserId = userId, Game = game, Rank = (Rating)Enum.Parse(typeof(Rating), rank, true), Region = region });
            }
            await _context.SaveChangesAsync();
        }

        public async Task RemovePlayerAsync(ulong serverId, ulong userId)
        {
            var player = await _context.Players
                .Where(x => (x.UserId == userId && x.ServerId == serverId))
                .FirstOrDefaultAsync();

            if(player != null)
                _context.Remove(player);
            
            await _context.SaveChangesAsync();
        }

        public async Task ClearPlayersAsync(List<Player> players)
        {
            _context.RemoveRange(players);
            await _context.SaveChangesAsync();
        }
    }
}