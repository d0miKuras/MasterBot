using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Infrastructure;

namespace MasterBot.Modules
{
    public class General : ModuleBase
    {
        private readonly ILogger<General> _logger;
        private readonly Servers _servers;
        public General(ILogger<General> logger, Servers servers) 
        {
            _logger = logger;
            _servers = servers;
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

        [Command("prefix")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Prefix(string prefix = null)
        {
            if(prefix == null)
            {
                var guildPrefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? "!"; // gets the prefix from the database or default prefix
                await ReplyAsync($"The current prefix of this bot is `{guildPrefix}`.");
                return;
            }

            if(prefix.Length > 8)
            {
                await ReplyAsync("The length of the new prefix is too long!");
                return;
            }

            await _servers.ModifyGuildPrefix(Context.Guild.Id, prefix);
            await ReplyAsync($"The prefix has been adjusted to `{prefix}`.");
        }
    }
}