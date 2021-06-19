using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using MasterBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MasterBot
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        public Startup(string[] args)
        {
            var builder = new ConfigurationBuilder()
                // .SetBasePath(AppContext.BaseDirectory) // bin folder
                .SetBasePath("/Users/d0mi/Projects/DiscordBots/MasterBot/MasterBot/MasterBot")
                .AddYamlFile("_config.yml");
            Configuration = builder.Build();
        }

        public static async Task RunAsync(string[] args)
        {
            var startup = new Startup(args);
            await startup.RunAsync();
        }

        public async Task RunAsync()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<CommandHandler>();

            await provider.GetRequiredService<StartupService>().StartAsync();
            await Task.Delay(-1);
        }


        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig 
            {
                LogLevel = Discord.LogSeverity.Verbose,
                MessageCacheSize = 1000
            }))
            .AddSingleton(new CommandService(new CommandServiceConfig
            {
                LogLevel = Discord.LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false
            }))
            .AddSingleton<CommandHandler>()
            .AddSingleton<StartupService>()
            .AddSingleton(Configuration);
        }
    }
}