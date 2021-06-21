using System.Linq;
using System.Reflection;
using System.Threading;
using System;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Discord.Commands;
using System.Threading.Tasks;
using Discord.Addons.Hosting;
using Discord;
using Infrastructure;
using MasterBot.Utilities;

namespace MasterBot.Services
{
    public class CommandHandler : InitializedService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfiguration _config;
        private readonly Servers _servers;
        private readonly AutoRolesHelper _autoRolesHelper;
        public CommandHandler(DiscordSocketClient discord, CommandService commands, IConfiguration config, IServiceProvider provider, Servers servers, AutoRolesHelper autoRolesHelper)
        {
            _provider = provider;
            _client = discord;
            _commands = commands;
            _config = config;
            _servers = servers;
            _autoRolesHelper = autoRolesHelper;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;
            _client.UserJoined += OnUserJoined;


            _commands.CommandExecuted += OnCommandExecuted;


            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

        }

        private async Task OnUserJoined(SocketGuildUser arg)
        {
            var roles = await _autoRolesHelper.GetAutoRolesAsync(arg.Guild);
            if(roles.Count < 1)
                return;
            
            await arg.AddRolesAsync(roles);
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
        
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            var prefix = await _servers.GetGuildPrefix((message.Channel as SocketGuildChannel).Guild.Id) ?? "!"; // fetches the prefix from the database or uses default prefix
            if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            await _commands.ExecuteAsync(context, argPos, _provider);
        

        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess) await context.Channel.SendMessageAsync($"Error: {result}");
        }
    }
}