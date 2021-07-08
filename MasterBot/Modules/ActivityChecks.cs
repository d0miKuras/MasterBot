using System.Xml.Linq;
using System.Linq;
using System;
using System.Threading.Tasks;
using Discord;
using Infrastructure;
using Discord.Commands;
using MasterBot.Utilities;
using Discord.WebSocket;
using Discord.Addons.Interactive;
using System.Collections.Generic;

namespace MasterBot.Modules
{

    public class ActivityChecks : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private readonly Servers _servers;
        public IEnumerable<IDMChannel> DMChannels { get; set; }
        public IEnumerable<IGuildUser> InactiveUsers { get; set; }
        public IEnumerable<IGuildUser> UsersOnGracePeriod { get; set; }   

        // public IGuildChannel LogChannel { get; set; }
        
        
        // public IMessage InactiveListMessage { get; set; }
        
        
        public ActivityChecks(DiscordSocketClient client, Servers servers)        
        {
            _client = client;
            _servers = servers;
        }
        public async Task<ITextChannel> GetLoggingChannelAsITextChannel(ulong id)
        {
            var logchannel = await Context.Guild.GetTextChannelAsync(await _servers.GetLogChannel(Context.Guild.Id)) as ITextChannel;
            return await Task.FromResult(logchannel);
        }
        /// <summary>
        /// Only call this from admin channel
        /// </summary>
        /// <returns></returns>
        [Command("activity-check", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Message()
        {

            try
            {
                var logchannel = await GetLoggingChannelAsITextChannel(Context.Guild.Id);
                var loggingOn = await _servers.GetLoggingOn(Context.Guild.Id);
                if(logchannel != null && loggingOn)
                {
                    await logchannel.SendMessageAsync($"{Context.User.Mention} has started an activity check.");
                }
                var message = await GetInactiveMembersAsync();
                foreach(var user in InactiveUsers)
                {
                        var directMessage = await user.SendMessageAsync($"Greetings! I have noticed that you have not been very active on the {Context.Guild} server. If you wish to continue being on the server, you might consider being more active. Once you have read this, please react with the ✅ reaction to start your grace period. If you won't send a message within a week, you will be kicked from the server.");
                        var emoji = new Emoji("✅");
                        await directMessage.AddReactionAsync(emoji);
                }
                DMChannels = await _client.GetDMChannelsAsync();
                _client.ReactionAdded += HandleReactionAddedAsync;

            }
            catch (Discord.Net.HttpException)
            {
                await Context.Channel.SendMessageAsync("User DM privacy restrictions prevent the bot from sending them messages.");
            }
            
        }

        private async Task HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel originChannel, SocketReaction reaction)
        {
            var message = await cachedMessage.GetOrDownloadAsync();
            var adminChannel = await Context.Guild.GetTextChannelAsync(await _servers.GetAdminChannel(Context.Guild.Id)); 
            var users = await originChannel.GetUsersAsync().FlattenAsync();
            var user = users.ElementAt(1);

            if(adminChannel == null)
                adminChannel = Context.Channel as ITextChannel;
            
            if(message != null && DMChannels.FirstOrDefault(x => x.Id == reaction.Channel.Id) != null && reaction.UserId == user.Id)
            {
                // await adminChannel.SendMessageAsync($"{user} has marked their activity.");
                await message.DeleteAsync();
                await message.Channel.SendMessageAsync($"Your grace period has started, try to show more activity before {DateTime.Now + new TimeSpan(7, 0, 0, 0)}");
                
                // InactiveUsers and UsersOnGracePeriod operations ////////////////////////
                var newList = InactiveUsers.ToList(); // to remove from inactive users

                var guildUser = newList.FirstOrDefault(x => x.Id == user.Id);
                newList.Remove(guildUser);
                InactiveUsers = newList;


                var graceList = UsersOnGracePeriod.ToList(); // to add to UsersOnGracePeriod
                graceList.Add(guildUser);
                UsersOnGracePeriod = graceList;
                ////////////////////////////////////////////////////////////////////////////
                
                var loggingChannel = await GetLoggingChannelAsITextChannel(Context.Guild.Id);
                var loggingOn = await _servers.GetLoggingOn(Context.Guild.Id);
                if(loggingChannel != null && loggingOn)
                {
                    await loggingChannel.SendMessageAsync($"{guildUser.Mention} has responded to the activity check, grace period started.");
                }


                await UpdateActivityMessage(DateTime.UtcNow + new TimeSpan(7, 0, 0, 0));

            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns the message that is posted to the admin channel that contains the list if inactive users</returns>
        private async Task<IUserMessage> GetInactiveMembersAsync()
        {
            var inactiveUsers = new List<IGuildUser>();
            var channels = await Context.Guild.GetTextChannelsAsync();
            var messages = new List<IMessage>();
            var adminChannel = await Context.Guild.GetTextChannelAsync(await _servers.GetAdminChannel(Context.Guild.Id));
            // var activityMessageId = _servers.GetActivityMessage(Context.Guild.Id);
            var activityMessageId = await _servers.GetActivityMessage(Context.Guild.Id);

            // var activityMessage = new IMessage();

            
            foreach(var channel in channels)
            {
                // var lastMessage = channel.SendMessageAsync("Activity check has started, this message will be deleted once the messages have been downloaded.");
                messages.AddRange(await channel.GetMessagesAsync(500).FlattenAsync());
            }
            


            var allUsers = await Context.Guild.GetUsersAsync();
            var currentTime = DateTimeOffset.Now;

            foreach (var user in allUsers)
            {
                var messagesByUser = messages.Where(x => x.Author == user); // filters by user
                // if(messagesByUser.All(x => (x.Timestamp - currentTime).Days > 14))
                if(!messagesByUser.All(x => (x.Timestamp - currentTime).Seconds > 10) && !user.IsBot)
                    inactiveUsers.Add(user);
            }
            InactiveUsers = inactiveUsers;
            string inactiveUserString = "List of inactive users:\n";
            foreach(var user in InactiveUsers)
                inactiveUserString += $"{user}\n";
            

            if(activityMessageId != 0)
            {
                var activityMessage = await adminChannel.GetMessageAsync(activityMessageId);
                await activityMessage.DeleteAsync();

            }
            var message = await adminChannel.SendMessageAsync(inactiveUserString);
            await message.PinAsync();
            await _servers.ModifyActivityMessage(Context.Guild.Id, message.Id);
            return await Task.FromResult(message);
        }

        private async Task UpdateActivityMessage(DateTime time)
        {
            var newMessage = "List of inactive users:\n";
            foreach(var user in InactiveUsers)
            {
                newMessage += $"{user}\n";
            }
            newMessage += "\nUsers on Grace Period:\n";
            foreach(var user in UsersOnGracePeriod)
            {
                newMessage += $"{user.Mention} -- Grace period end: {time} UTC\n";
            }

            var messageId = await _servers.GetActivityMessage(Context.Guild.Id);
            // var channel = await Context.Guild.GetChannelAsync(await _servers.GetAdminChannel(Context.Guild.Id));
            await Context.Channel.ModifyMessageAsync(messageId, (x => x.Content = newMessage));


        }

    }
}