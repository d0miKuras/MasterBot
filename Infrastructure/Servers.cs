using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class Servers
    {
        private readonly MasterBotContext _context;

        public Servers(MasterBotContext context)
        {
            _context = context;
        }

        public async Task ModifyGuildPrefix(ulong id, string prefix)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if(server == null)
                _context.Add(new Server {Id = id, Prefix = prefix});
            else
                server.Prefix = prefix;

            await _context.SaveChangesAsync();
        }

        public async Task<String> GetGuildPrefix(ulong id)
        {
            var prefix = await _context.Servers
                .Where(x => x.Id == id)
                .Select(x => x.Prefix)
                .FirstOrDefaultAsync();
            
            return await Task.FromResult(prefix);
        }

        public async Task ModifyAdminChannel(ulong serverId, ulong channelId)
        {
            var server = await _context.Servers
                .FindAsync(serverId);
            
            if(server == null)
                _context.Add(new Server {Id = serverId, AdminChannel = channelId});
            else
                server.AdminChannel = channelId;
            
            await _context.SaveChangesAsync();
        }

        public async Task<ulong> GetAdminChannel(ulong id)
        {
            var adminChannel = await _context.Servers
                .Where(x => x.Id == id)
                .Select(x => x.AdminChannel)
                .FirstOrDefaultAsync();

            
            
            return await Task.FromResult(adminChannel);
        }

        // public async Task<List<ulong>> GetInactiveMembers(ulong id)
        // {
        //     var inactiveMembers = 
        //         from server in _context.Servers
        //         where server.Id == id
        //         select server.InactiveMembers;
        //     // var inactiveMembers = _context.Servers
        //     //     .Where(x => x.Id == id)
        //     //     .Select(x => x.InactiveMembers);
        //     // var ret = inactiveMembers as List<ulong>;
        //     return await Task.FromResult(await inactiveMembers.FirstOrDefaultAsync());
        // }

        // public async Task ModifyInactiveMembers(ulong id, List<ulong> users)
        // {
        //     var server = await _context.Servers
        //         .FindAsync(id);

        //     if(server == null)
        //         _context.Add(new Server {Id = id, InactiveMembers = users});
        //     else
        //         server.InactiveMembers = users;
            
        //     await _context.SaveChangesAsync();
        // }

        public async Task<ulong> GetActivityMessage(ulong id)
        {
            var activityMessage =
                from server in _context.Servers
                where server.Id == id
                select server.ActivityMessage;
            
            return await Task.FromResult(await activityMessage.FirstOrDefaultAsync());
        }


        public async Task ModifyActivityMessage(ulong id, ulong activityMessage)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if(server == null)
                _context.Add(new Server {Id = id, ActivityMessage = activityMessage});
            else
                server.ActivityMessage = activityMessage;
            
            await _context.SaveChangesAsync();    
        }
    }
}