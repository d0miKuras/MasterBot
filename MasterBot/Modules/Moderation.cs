using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace MasterBot.Modules
{
    public class Moderation : ModuleBase
    {
        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Purge(int amount) // DELETES amount MESSAGES IF PERMISISONS ARE RIGHT
        {
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync(); // amount + 1 also removes the message that called for purge
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

            var message = await Context.Channel.SendMessageAsync($"{messages.Count()} messages deleted successfully!");
            await Task.Delay(2500); // wait 2.5 seconds
            await message.DeleteAsync();
        }
    }
}
