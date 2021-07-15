using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class Users
    {
        private readonly MasterBotContext _context;

        public Users(MasterBotContext context)
        {
            _context = context;
        }

        public async Task<DateTime> GetLastMessageDate(ulong id, ulong guildId)
        {
            var date = 
                from user in _context.Users
                where user.UserId == id && user.GuildId == guildId
                select user.LastActivity;
            
            return await Task.FromResult(await date.FirstOrDefaultAsync());
        }

        public async Task UpdateLastMessageDateAsync(ulong id, ulong guildId)
        {
            var user = await _context.Users.Where(x => x.UserId == id && x.GuildId == guildId).FirstOrDefaultAsync();

            if(user == null)
                _context.Add(new User{UserId = id, GuildId = guildId, LastActivity = DateTime.UtcNow});
            else
                user.LastActivity = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

    }
}