using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class AutoRoles
    {
        private readonly MasterBotContext _context;

        public AutoRoles(MasterBotContext context)
        {
            _context = context;
        }

        public async Task<List<AutoRole>> GetAutoRolesAsync(ulong serverId) // returns a list of autoroles on the given server
        {
            var autoRoles = await _context.AutoRoles
                .Where(x => x.ServerId == serverId)
                .ToListAsync();

            return await Task.FromResult(autoRoles);
        }

        public async Task AddAutoRoleAsync(ulong serverId, ulong roleId) // adds a given autorole to the given server
        {
            var server = await _context.Servers
                .FindAsync(serverId);

            if(server == null)
                _context.Add(new Server {Id = serverId});
            
            _context.Add(new AutoRole {RoleId = roleId, ServerId = serverId});
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAutoRoleAsync(ulong serverId, ulong roleId)
        {
            var autoRole = await _context.AutoRoles
                .Where(x => x.RoleId == roleId)
                .FirstOrDefaultAsync();
            
            _context.Remove(autoRole);
            await _context.SaveChangesAsync();
        }

        public async Task ClearAutoRolesAsync(List<AutoRole> autoRoles)
        {
            _context.RemoveRange(autoRoles);
            await _context.SaveChangesAsync();
        }
    }
}