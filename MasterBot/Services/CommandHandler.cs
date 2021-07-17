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
using Victoria;

namespace MasterBot.Services
{
    public class CommandHandler : InitializedService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfiguration _config;
        private readonly Servers _servers;
        private readonly Users _users;
        private readonly AutoRolesHelper _autoRolesHelper;
        private readonly LavaNode _lavaNode;
        private readonly BannedWords _bannedWords;
        public CommandHandler(DiscordSocketClient discord, CommandService commands, IConfiguration config, 
                            IServiceProvider provider, Servers servers, Users users, AutoRolesHelper autoRolesHelper, 
                            LavaNode lavaNode, BannedWords bannedWords)
        {
            _provider = provider;
            _client = discord;
            _commands = commands;
            _config = config;
            _servers = servers;
            _users = users;
            _autoRolesHelper = autoRolesHelper;
            _lavaNode = lavaNode;
            _bannedWords = bannedWords;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;
            // _client.MessageReceived += MessageReceivedUpdateDb;
            _client.UserJoined += OnUserJoined;
            _client.Ready += OnReadyAsync;
            _commands.CommandExecuted += OnCommandExecuted;


            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

        }
        
        private async Task OnReadyAsync() 
        {
	        if (!_lavaNode.IsConnected)
            {
			    await _lavaNode.ConnectAsync();
		    }
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
            // await Task.Run(async() => await _users.UpdateLastMessageDateAsync(message.Author.Id, (message.Channel as SocketGuildChannel).Guild.Id));
            await _users.UpdateLastMessageDateAsync(message.Author.Id, (message.Channel as SocketGuildChannel).Guild.Id);


            var argPos = 0;
            var prefix = await _servers.GetGuildPrefix((message.Channel as SocketGuildChannel).Guild.Id) ?? "!"; // fetches the prefix from the database or uses default prefix
            var bannedWords = await _bannedWords.GetBannedWordsAsync((message.Channel as SocketGuildChannel).Guild.Id);
            foreach(var word in bannedWords)
            {
                if(message.Content.Contains(word) && !message.HasStringPrefix(prefix, ref argPos))
                {
                    await message.Channel.SendMessageAsync($"{message.Author.Mention}, The message contains a banned word. Please refrain from using that word.");
                    await message.DeleteAsync();
                }
            }
            if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;
            
            
            var context = new SocketCommandContext(_client, message);
            await _commands.ExecuteAsync(context, argPos, _provider);
        }

        // private Task MessageReceivedUpdateDb(SocketMessage message)
        // {
        //     _ = Task.Run(async() => await  _users.UpdateLastMessageDateAsync(message.Author.Id, (message.Channel as SocketGuildChannel).Guild.Id));
        //     return Task.CompletedTask;
        // }


        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess) await context.Channel.SendMessageAsync($"Error: {result}");
        }
    }
}