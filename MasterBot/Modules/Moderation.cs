using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure;
using MasterBot.Utilities;

namespace MasterBot.Modules
{
    public class Moderation : ModuleBase
    {

        private readonly Servers _servers;
        private readonly RanksHelper _ranksHelper;
        private readonly BannedWords _bannedWords;

        public Moderation(Servers servers, RanksHelper ranksHelper, BannedWords bannedWords)
        {
            _servers = servers;
            _ranksHelper = ranksHelper;
            _bannedWords = bannedWords;
        }
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

        [Command("ban")]
        [Alias("b")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Ban(IGuildUser user, [Remainder]string reason)
        {
            if((user as SocketGuildUser).Hierarchy > (Context.Guild as SocketGuild).CurrentUser.Hierarchy)
            {
                await ReplyAsync("Cannot ban a user higher in hierarchy than you.");
                return;
            }
            var logChannelId = await _servers.GetLogChannel(Context.Guild.Id);
            var logChannel = await Context.Guild.GetChannelAsync(logChannelId) as ITextChannel;
            var loggingOn = await _servers.GetLoggingOn(Context.Guild.Id);

            
            if(logChannel != null && loggingOn)
                await logChannel.SendMessageAsync($"{user.Mention} is banned by {Context.User.Mention}. Reason: `{reason}`");
            await Context.Guild.AddBanAsync(user, reason: reason);
        }

        [Command("unban")]
        [Alias("ub")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Unban(string name)
        {
            var bannedUsers = await Context.Guild.GetBansAsync();
            var ban = bannedUsers.FirstOrDefault(x => x.User.Username == name);
            if(ban == null)
            {
                await ReplyAsync("A user with this name is not banned.");
                return;
            }

            await Context.Guild.RemoveBanAsync(ban.User.Id);
            await ReplyAsync($"{ban.User.Mention} unbanned");
        }
        

        [Command("kick")]
        [Alias("k")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Kick(IGuildUser user, [Remainder]string reason)
        {
            if((user as SocketGuildUser).Hierarchy > (Context.Guild as SocketGuild).CurrentUser.Hierarchy)
            {
                await ReplyAsync("Cannot ban a user higher in hierarchy than you.");
                return;
            }
            var logChannelId = await _servers.GetLogChannel(Context.Guild.Id);
            var logChannel = await Context.Guild.GetChannelAsync(logChannelId) as ITextChannel;
            var loggingOn = await _servers.GetLoggingOn(Context.Guild.Id);


            if(logChannel != null && loggingOn)
                await logChannel.SendMessageAsync($"{user.Mention} is kicked by {Context.User.Mention}. Reason: `{reason}`");
            await user.KickAsync(reason);
        }

        [Command("add-banned-word", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AddBannedWord(string word)
        {
            await _bannedWords.AddBannedWordAsync(Context.Guild.Id, word);
            await ReplyAsync($"Added `{word}` to the list of banned words.");
        }


        [Command("remove-banned-word", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveBannedWord(string word)
        {
            await _bannedWords.RemoveBannedWordAsync(Context.Guild.Id, word);
            await ReplyAsync($"Removed `{word}` from the list of banned words");
        }
    }
}
