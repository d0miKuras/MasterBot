using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MasterBot.Services;
using Infrastructure;
using MasterBot.Utilities;
using Victoria;
using Interactivity;

namespace MasterBot
{
    class Program
    {
        static async Task Main()
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(x => // Config file
                {
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", false, true)
                        .Build();

                    x.AddConfiguration(configuration);
                })
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel.Debug); // Defines logging level
                })
                .ConfigureDiscordHost((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Verbose,
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 200
                    };
                    config.Token = context.Configuration["token"];
                })
                .UseCommandService((context, config) =>
                {
                    config = new CommandServiceConfig()
                    {
                        CaseSensitiveCommands = false,
                        LogLevel = LogSeverity.Verbose,
                        // DefaultRunMode = RunMode.Async
                    };
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<CommandHandler>()
                    .AddDbContext<MasterBotContext>()
                    .AddLavaNode(x =>
                    {
                        x.SelfDeaf = true;
                    })
                    .AddSingleton<InteractivityService>()
                    .AddSingleton<Servers>()
                    .AddSingleton<Ranks>()
                    .AddSingleton<AutoRoles>()
                    .AddSingleton<LFGs>()
                    .AddSingleton<Games>()
                    .AddSingleton<Users>()
                    .AddSingleton<Players>()
                    .AddSingleton<RanksHelper>()
                    .AddSingleton<AutoRolesHelper>()
                    .AddSingleton<PlayerHelper>()
                    .AddSingleton<LFGHelper>()
                    .AddSingleton<GameHelper>();
                })
                .UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
                
        }
    }
}