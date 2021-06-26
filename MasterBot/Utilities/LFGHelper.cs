using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Infrastructure;
using Discord.WebSocket;

namespace MasterBot.Utilities
{
    public class LFGHelper
    {
        private readonly LFGs _lfgs;
        private readonly Games _games;
        private readonly Players _players;

        public LFGHelper(LFGs lfgs, Games game, Players players)
        {
            _lfgs = lfgs;
            _games = game;
            _players = players;
        }
        /// <summary>
        /// A helper used to find the list of users from the server that are looking for the game with the same mode.
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="gameName"></param>
        /// <returns>List of Player that are looking for the given game in with the same mode</returns>
        public async Task<List<Player>> GetLFGsAsync(IGuild guild, string gameName, Mode mode)
        {
            var game = await _games.GetGameAsync(gameName);
            List<LFG> lfgsOnServer = new List<LFG>();
            List<LFG> invalidLFGs = new List<LFG>();
            List<Player> users = new List<Player>();
            if(game == null)
                lfgsOnServer = await _lfgs.GetLFGsAsync(guild.Id);
            else
                lfgsOnServer = await _lfgs.GetLFGsAsync(guild.Id, game);

            var players = await _players.GetPlayersAsync(guild.Id);
            // Checks if there are players registered at all
            if(players.Count > 0)
            {
                foreach(var lfg in lfgsOnServer)
                {
                    var user = await guild.GetUserAsync(lfg.Player.UserId);
                    if(user == null)
                    {
                        invalidLFGs.Add(lfg);
                        // lfgsOnServer.Remove(lfg); 
                    }
                }
                if(invalidLFGs.Count > 0)
                    await _lfgs.ClearLFGsAsync(invalidLFGs);
            
            
                foreach(var lfg in lfgsOnServer)
                {
                    if(lfg.Mode == mode)
                        users.Add(lfg.Player);
                }
            }
            return await Task.FromResult(users);

        }
    }
}