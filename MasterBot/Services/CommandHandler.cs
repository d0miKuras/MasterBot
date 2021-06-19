using System;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Discord.Commands;
using System.Threading.Tasks;

namespace MasterBot.Services
{
    public class CommandHandler
    {
        public static IServiceProvider _provider;
        public static DiscordSocketClient _discord;
        public static CommandService _commands;
        public static IConfigurationRoot _config;
        public CommandHandler(DiscordSocketClient discord, CommandService commands, IConfigurationRoot config, IServiceProvider provider)
        {
            _provider = provider;
            _discord = discord;
            _commands = commands;
            _config = config;

            _discord.Ready += OnReady;
            _discord.MessageReceived += OnMessageReceived;
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if(msg.Author.IsBot) return; // does nothing if the author is bot

            var context = new SocketCommandContext(_discord, msg);

            int pos = 0;
            if(msg.HasStringPrefix(_config["prefix"], ref pos) || msg.HasMentionPrefix(_discord.CurrentUser, ref pos)) // ensures the message contains the prefix or hte bot is tagged
            {
                var result = await _commands.ExecuteAsync(context, pos, _provider); // sends context, pos and provider to commands

                if(!result.IsSuccess)
                {
                    var reason = result.Error;

                    await context.Channel.SendMessageAsync($"The following error has occurred:\n{reason}"); // posts the error to the chat
                    Console.WriteLine(reason); // logs error to console, need to expand it
                }
            }

        }

        private Task OnReady()
        {
            Console.WriteLine($"Connected as {_discord.CurrentUser.Username}#{_discord.CurrentUser.Discriminator}");
            return Task.CompletedTask;
        }
    }
}