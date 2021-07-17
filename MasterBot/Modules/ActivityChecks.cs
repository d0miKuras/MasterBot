using System.Linq;
using System;
using System.Threading.Tasks;
using Discord;
using Infrastructure;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using Interactivity;
using Interactivity.Confirmation;

namespace MasterBot.Modules
{

    public class ActivityChecks : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private readonly Servers _servers;
        private readonly Users _users;

        private readonly InteractivityService _interactive;
        public IEnumerable<IDMChannel> DMChannels { get; set; }
        public IEnumerable<IGuildUser> InactiveUsers { get; set; }
        public IEnumerable<IGuildUser> UsersOnGracePeriod { get; set; }           
        
        public ActivityChecks(DiscordSocketClient client, Servers servers, InteractivityService inter, Users users)        
        {
            _client = client;
            _servers = servers;
            _interactive = inter;
            _users = users;
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
                var timeout = await _servers.GetInactivityPeriod(Context.Guild.Id);
                // var timeout = 1;
                foreach(var user in InactiveUsers)
                {
                    var request = new ConfirmationBuilder()
                        .WithContent(new PageBuilder().WithText($"Greetings! I have noticed that you have not been active on the {Context.Guild} server in the past {timeout} days. If you wish to continue being on the server, you might consider being more active. Once you have read this, please react with the ✅ reaction to start your grace period. If you won't send a message within a week, you will be kicked from the server."))
                        .Build();
                    var dm = await user.SendMessageAsync("hi");
                    var channel = dm.Channel;
                    await dm.DeleteAsync();
                    var result = await _interactive.SendConfirmationAsync(request, channel, new TimeSpan(timeout*24, 0, 0));
                    if(result.Value)
                        await HandleReactionAddedAsync(channel);
                }

            }
            catch (Discord.Net.HttpException)
            {
                await Context.Channel.SendMessageAsync("User DM privacy restrictions prevent the bot from sending them messages.");
            }
            
        }

        private async Task HandleReactionAddedAsync(IMessageChannel channel)
        {
            var adminChannel = await Context.Guild.GetTextChannelAsync(await _servers.GetAdminChannel(Context.Guild.Id));

            await channel.SendMessageAsync($"Your grace period has started, try to show more activity before {DateTime.Now + new TimeSpan(7, 0, 0, 0)}");

            var user = await channel.GetUsersAsync().FlattenAsync();

            // InactiveUsers and UsersOnGracePeriod operations ////////////////////////
            var newList = InactiveUsers.ToList(); // to remove from inactive users

            var guildUser = newList.FirstOrDefault(x => x.Id == user.ElementAt(1).Id);
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
            var inactivePeriod = await _servers.GetInactivityPeriod(Context.Guild.Id);
            var adminChannel = await Context.Guild.GetTextChannelAsync(await _servers.GetAdminChannel(Context.Guild.Id));
            var activityMessageId = await _servers.GetActivityMessage(Context.Guild.Id);
            


            var allUsers = await Context.Guild.GetUsersAsync();
            var currentTime = DateTimeOffset.UtcNow;


            foreach (var user in allUsers)
            {
                if(!user.IsBot)
                {
                    var lastMessageDate = await _users.GetLastMessageDate(user.Id, Context.Guild.Id);
                    if(lastMessageDate == DateTime.MinValue || (currentTime - lastMessageDate).Days > inactivePeriod) // lastMessageDate == DateTime.MinValue means user is not registered, so hasnt sent a message
                    // and hence is not registered in the database yet
                        inactiveUsers.Add(user);
                }
                

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
            await Context.Channel.ModifyMessageAsync(messageId, (x => x.Content = newMessage));
        }

    }
}