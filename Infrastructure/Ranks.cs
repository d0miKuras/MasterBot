using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class Ranks
    {
        private readonly MasterBotContext _context;

        public Ranks(MasterBotContext context)
        {
            _context = context;
        }

        public async Task<List<Rank>> GetRanksAsync(ulong serverId) // returns a list of ranks on the given server
        {
            var ranks = await _context.Ranks
                .Where(x => x.ServerId == serverId)
                .ToListAsync();

            return await Task.FromResult(ranks);
        }

        public async Task AddRankAsync(ulong serverId, ulong roleId) // adds a given role to the given server
        {
            var server = await _context.Servers
                .FindAsync(serverId);

            if(server == null)
                _context.Add(new Server {Id = serverId});
            
            _context.Add(new Rank {RoleId = roleId, ServerId = serverId});
            await _context.SaveChangesAsync();
        }

        public async Task RemoveRankAsync(ulong serverId, ulong roleId)
        {
            var rank = await _context.Ranks
                .Where(x => x.RoleId == roleId)
                .FirstOrDefaultAsync();
            
            _context.Remove(rank);
            await _context.SaveChangesAsync();
        }

        public async Task ClearRanksAsync(List<Rank> ranks)
        {
            _context.RemoveRange(ranks);
            await _context.SaveChangesAsync();
        }
    }
}