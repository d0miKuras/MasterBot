using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Infrastructure;
using MasterBot.Utilities;

namespace MasterBot.Modules
{
    public class General : ModuleBase
    {
        private readonly ILogger<General> _logger;
        private readonly RanksHelper _ranksHelper;
        public General(ILogger<General> logger, RanksHelper ranksHelper) 
        {
            _logger = logger;
            _ranksHelper = ranksHelper;
        }

        [Command("ping")]
        public async Task Ping()
        {
            await Context.Channel.SendMessageAsync("Pong!");
            _logger.LogInformation($"{Context.User} executed the ping command!");
        }

        [Command("info")]
        public async Task Info(SocketGuildUser user = null) // Displays information about the user
        {

            user ??= (SocketGuildUser)Context.User;
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .WithDescription($"In this message you can see information about {user}!")
                .WithColor(new Color(new Random().Next(0, 255), new Random().Next(0, 255), new Random().Next(0, 255)))
                .AddField("User ID", Context.User.Id, true)
                .AddField("Discriminator", user.Discriminator, true)
                .AddField("Created at", user.CreatedAt.ToString("dd/MM/yyyy"), true)
                .AddField("Joined at", (user as SocketGuildUser).JoinedAt.Value.ToString("dd/MM/yyyy"), true)
                .AddField("Roles", string.Join(", ", (user as SocketGuildUser).Roles.Select(x => x.Mention)))
                .WithCurrentTimestamp();
            
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }


        [Command("server")]
        public async Task Server() // Displays information about the server
        {
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription("Information about the server.")
                .WithTitle($"{Context.Guild.Name} Information")
                .WithColor(new Color(new Random().Next(0, 255), new Random().Next(0, 255), new Random().Next(0, 255)))
                .AddField("Created at", Context.Guild.CreatedAt.ToString("dd/MM/yyyy"))
                .AddField("Member count", (Context.Guild as SocketGuild).MemberCount + " members", true)
                .AddField("Online Users", (Context.Guild as SocketGuild).Users.Where(x => x.Status != UserStatus.Offline).Count() + " members", true);

            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }

        [Command("rank")] // adds or removes given rank
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Rank([Remainder]string identifier) // user can use either roleId or roleName
        {
            await Context.Channel.TriggerTypingAsync(); // adds typing indicator for the user to know that the bot is working on the solution
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

            IRole role; // uninitialized

            if(ulong.TryParse(identifier, out ulong roleId)) // tries to parse identifier as ulong, if succeeds roleId is the result
            {
                var roleById = Context.Guild.Roles.FirstOrDefault(x => x.Id == roleId); // finds the role by id
                if(roleById == null)
                {
                    await ReplyAsync("This role does not exist!");
                    return;
                }
                role = roleById;
            }
            else
            {
                var roleByName = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, identifier, StringComparison.CurrentCultureIgnoreCase)); // finds role by name
                if(roleByName == null)
                {
                    await ReplyAsync("This role does not exist!");
                    return;
                }
                role = roleByName;
            }

            if(ranks.All(x => x.Id != role.Id))
            {
                await ReplyAsync("This rank does not exist!");
                return;
            }
            if((Context.User as SocketGuildUser).Roles.Any(x => x.Id == role.Id)) // if the user already has this role, removes it
            {
                await (Context.User as SocketGuildUser).RemoveRoleAsync(role);
                await ReplyAsync($"Successfully removed the role {role.Mention} from you.");
                return;
            }
            await (Context.User as SocketGuildUser).AddRoleAsync(role); // if the user doesnt have it added yet, gives it 
            await ReplyAsync($"Successfully added role {role.Mention} to you.");
        }
    }
}