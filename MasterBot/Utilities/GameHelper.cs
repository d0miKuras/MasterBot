using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Infrastructure;
using Discord.WebSocket;
namespace MasterBot.Utilities
{
    public class GameHelper
    {
        private readonly Games _games;

        public GameHelper(Games games)
        {
            _games = games;
        }

        public async Task<List<Infrastructure.Game>> GetGamesAsync()
        {
            return await _games.GetGamesAsync();
        }
    }
}
