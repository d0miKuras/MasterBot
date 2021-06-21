using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Infrastructure;
using Discord.WebSocket;

namespace MasterBot.Utilities
{
    public class RanksHelper
    {
        private readonly Ranks _ranks;

        public RanksHelper(Ranks ranks)
        {
            _ranks = ranks;
        }

        public async Task<List<IRole>> GetRanksAsync(IGuild guild)
        {
            var roles = new List<IRole>(); // list of all roles
            var invalidRanks = new List<Rank>(); // list of invalid roles (admin deleted but the DB was not updated)

            var ranks = await _ranks.GetRanksAsync(guild.Id); // gets ranks by server id

            foreach(var rank in ranks)
            {
                var role = guild.Roles.FirstOrDefault(x => x.Id == rank.RoleId);
                if(role == null) // role not in the list
                    invalidRanks.Add(rank);
                else
                {
                    var currentUser = await guild.GetCurrentUserAsync(); // bot
                    var hierarchy = (currentUser as SocketGuildUser).Hierarchy; // hierarchy of the bot


                    if(role.Position > hierarchy) // if the current role is higher in hierarchy, then its invalid
                        invalidRanks.Add(rank);
                    else
                        roles.Add(role);
                }
            }

            if(invalidRanks.Count > 0) // if there are some invalid roles, update DB
                await _ranks.ClearRanksAsync(invalidRanks);

            return roles;
        }
    }
}