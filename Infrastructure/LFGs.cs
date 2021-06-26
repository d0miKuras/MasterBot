using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class LFGs
    {
        private readonly MasterBotContext _context;

        public LFGs(MasterBotContext context)
        {
            _context = context;
        }

        public async Task<List<LFG>> GetLFGsAsync(ulong serverId)
        {
            var lfgs = await _context.LFGs
                .Where(x => x.ServerId == serverId)
                .ToListAsync();
            return await Task.FromResult(lfgs);
        }

        public async Task<List<LFG>> GetLFGsAsync(ulong serverId, Game game)
        {
            var lfgs = await _context.LFGs
                .Where(x => (x.ServerId == serverId && x.Game == game))
                .ToListAsync();
            
            return await Task.FromResult(lfgs);
        }

        public List<LFG> GetLFGs(ulong serverId, Game game)
        {
            var lfgs = _context.LFGs
                .Where(x => (x.ServerId == serverId && x.Game == game))
                .ToList();
            return lfgs;
        }

        public async Task AddLFGAsync(ulong serverId, Game game, Player player, Mode mode, Rating lowerBound, Rating upperBound)
        {
            var lfgs = await GetLFGsAsync(serverId, game);
            var existingLfg = lfgs.FirstOrDefault(x=> (x.ServerId == serverId && x.Game == game && x.Player == player && x.Mode == mode)); // checks if the entry given is already in the database
            if(existingLfg == null)
                _context.Add(new LFG{ServerId = serverId, Game = game, Player = player, Mode = mode, LowerRatingBound = lowerBound, UpperRatingBound = upperBound});
            else // if the lfg is already in the database, remove the old entry and add a new one
            {
                _context.Remove(existingLfg);
                _context.Add(new LFG{ServerId = serverId, Game = game, Player = player, Mode = mode, LowerRatingBound = lowerBound, UpperRatingBound = upperBound});
            }
            await _context.SaveChangesAsync();
        }

        public async Task AddLFGAsync(LFG lfg)
        { 
            _context.Add(lfg);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveLFGAsync(ulong serverId, Game game, Player player, Mode mode)
        {
            var lfg = await _context.LFGs
                .FirstOrDefaultAsync(x => (x.ServerId == serverId && x.Game == game && x.Player == player && x.Mode == mode));

            if(lfg != null)
                _context.Remove(lfg);
            
            await _context.SaveChangesAsync();
        }

        public async Task ClearLFGsAsync(List<LFG> lfgs)
        {
            _context.RemoveRange(lfgs);
            await _context.SaveChangesAsync();
        }
    }
}
