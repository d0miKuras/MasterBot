using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Interactivity;
using Interactivity.Pagination;
using Discord.WebSocket;
using Interactivity.Confirmation;

namespace MasterBot.Modules
{
    public class TestModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;

        private readonly InteractivityService _interactive;

        public TestModule(DiscordSocketClient client, InteractivityService interactive)
        {
            _client = client;
            _interactive = interactive;
        }

        [Command("confirm", RunMode = RunMode.Async)]
        public async Task ConfirmAsync()
        {
            var request = new ConfirmationBuilder()
            .WithContent(new PageBuilder().WithText("Please Confirm"))
            .Build();
            var dm = await Context.User.SendMessageAsync("hi");
            var result = await _interactive.SendConfirmationAsync(request, dm.Channel);

            if (result.Value)
            {
                await Context.Channel.SendMessageAsync("Confirmed :thumbsup:!");
            }
            else
            {
                await Context.Channel.SendMessageAsync("Declined :thumbsup:!");
            }
        }
    }
}