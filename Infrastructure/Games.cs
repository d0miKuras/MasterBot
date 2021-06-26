using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class Games
    {
        private readonly MasterBotContext _context;
        public Games(MasterBotContext context)
        {
            _context = context;
        }

        public async Task<List<Game>> GetGamesAsync()
        {
            var games = await _context.Games.ToListAsync();

            return await Task.FromResult(games);
        }

/// <summary>
/// </summary>
/// <param name="name"></param>
/// <returns>Returns a game with the given Name or ShortName.</returns>
        public async Task<Game> GetGameAsync(string name)
        {
            var game = await _context.Games.
                FirstOrDefaultAsync(x => string.Equals(x.Name, name) || string.Equals(x.ShortName, name));
            return await Task.FromResult(game);
        }
/// <summary>
/// Adds a Game with the given name, shortName, integerRanked to the database.
/// </summary>
/// <param name="name"></param>
/// <param name="short_name"></param>
/// <param name="integerRanked"></param>
/// <returns></returns>
        public async Task AddGameAsync(string name, string short_name, bool integerRanked)
        {
            var game = await _context.Games.FirstOrDefaultAsync(x => string.Equals(x.Name, name));
            if(game == null)
                _context.Add(new Game {Name = name, ShortName = short_name, IntegerRankingSystem = integerRanked});
            await _context.SaveChangesAsync();
        }
    }
}