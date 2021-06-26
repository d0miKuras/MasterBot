using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Infrastructure;
using Discord.WebSocket;

namespace MasterBot.Utilities
{
    public class PlayerHelper
    {
        private readonly Players _players;
        public PlayerHelper(Players players)
        {
            _players = players;
        }

        public async Task<List<IGuildUser>> GetPlayersAsGuildUsersAsync(IGuild guild)
        {
            List<IGuildUser> validUsers = new List<IGuildUser>();
            List<Player> invalidPlayers = new List<Player>();

            var players = await _players.GetPlayersAsync(guild.Id);

            foreach(var player in players)
            {
                if((player as IGuildUser) != guild.GetUserAsync(player.UserId))
                    invalidPlayers.Add(player);
                else
                    validUsers.Add(player as IGuildUser);
            }
            if(invalidPlayers.Count > 0)
                await _players.ClearPlayersAsync(invalidPlayers);
            
            return validUsers;
        }
        public async Task<List<Player>> GetPlayersAsync(IGuild guild)
        {
            List<Player> validUsers = new List<Player>();
            List<Player> invalidPlayers = new List<Player>();

            var players = await _players.GetPlayersAsync(guild.Id);

            foreach(var player in players)
            {
                if(guild.GetUserAsync(player.UserId) == null)
                    invalidPlayers.Add(player);
                else
                    validUsers.Add(player);
            }
            if(invalidPlayers.Count > 0)
                await _players.ClearPlayersAsync(invalidPlayers);
            
            return await Task.FromResult(validUsers);
        }
    }
}