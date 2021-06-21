using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Infrastructure;
using Discord.WebSocket;

namespace MasterBot.Utilities
{
    public class AutoRolesHelper
    {
        private readonly AutoRoles _autoRoles;

        public AutoRolesHelper(AutoRoles autoRoles)
        {
            _autoRoles = autoRoles;
        }

        public async Task<List<IRole>> GetAutoRolesAsync(IGuild guild)
        {
            var roles = new List<IRole>(); // list of all roles
            var invalidAutoRoles = new List<AutoRole>(); // list of invalid roles (admin deleted but the DB was not updated)

            var autoRoles = await _autoRoles.GetAutoRolesAsync(guild.Id); // gets AutoRoles by server id

            foreach(var autoRole in autoRoles)
            {
                var role = guild.Roles.FirstOrDefault(x => x.Id == autoRole.RoleId);
                if(role == null) // role not in the list
                    invalidAutoRoles.Add(autoRole);
                else
                {
                    var currentUser = await guild.GetCurrentUserAsync(); // bot
                    var hierarchy = (currentUser as SocketGuildUser).Hierarchy; // hierarchy of the bot


                    if(role.Position > hierarchy) // if the current role is higher in hierarchy, then its invalid
                        invalidAutoRoles.Add(autoRole);
                    else
                        roles.Add(role);
                }
            }

            if(invalidAutoRoles.Count > 0) // if there are some invalid roles, update DB
                await _autoRoles.ClearAutoRolesAsync(invalidAutoRoles);

            return roles;
        }
    }
}