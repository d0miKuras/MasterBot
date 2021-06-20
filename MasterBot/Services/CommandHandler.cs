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

namespace MasterBot.Services
{
    public class CommandHandler : InitializedService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfiguration _config;
        public CommandHandler(DiscordSocketClient discord, CommandService commands, IConfiguration config, IServiceProvider provider)
        {
            _provider = provider;
            _client = discord;
            _commands = commands;
            _config = config;

        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;
            _commands.CommandExecuted += OnCommandExecuted;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

        }
        private async Task OnMessageReceived(SocketMessage arg)
        {
        
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            if (!message.HasStringPrefix(_config["prefix"], ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            await _commands.ExecuteAsync(context, argPos, _provider);
        

        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess) await context.Channel.SendMessageAsync($"Error: {result}");
        }
    }
}