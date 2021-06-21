using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Infrastructure;
using Discord.WebSocket;
using Discord.Commands;
using MasterBot.Utilities;

namespace MasterBot.Modules
{
    public class Configuration : ModuleBase<SocketCommandContext>
    {
        private readonly RanksHelper _ranksHelper;
        private readonly Servers _servers;
        private readonly Ranks _ranks;
        private readonly AutoRolesHelper _autoRolesHelper;
        private readonly AutoRoles _autoRoles;

        public Configuration(RanksHelper ranksHelper, Servers servers, Ranks ranks, AutoRolesHelper autoRolesHelper, AutoRoles autorolls)
        {
            _ranksHelper = ranksHelper;
            _autoRolesHelper = autoRolesHelper;
            _servers = servers;
            _ranks = ranks;
            _autoRoles = autorolls;
        }

        [Command("prefix", RunMode = RunMode.Async)]
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

        [Command("ranks", RunMode = RunMode.Async)]
        public async Task Ranks()
        {
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

            if(ranks.Count == 0)
            {
                await ReplyAsync("This server does not yet have any ranks!");
                return;
            }

            await Context.Channel.TriggerTypingAsync(); // adds typing indicator for the user to know that the bot is working on the solution

            string description = "This message lists all available ranks.\nIn order to add a rank, you can use the name or ID of the rank (if two or more ranks have the same name)";
            foreach(var rank in ranks)
            {
                description += $"\n{rank.Mention} ({rank.Id})";
            }
            await ReplyAsync(description);
        }

        [Command("delete-rank", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task DeleteRank([Remainder]string name)
        {
            await Context.Channel.TriggerTypingAsync(); // adds typing indicator for the user to know that the bo is working on the solution
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);
            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if(role == null)
            {
                await ReplyAsync("This role does not exist!");
                return;
            }
            if(role.Position > Context.Guild.CurrentUser.Hierarchy) // prevents bot from removing roles higher than its own
            {
                await ReplyAsync("That role is higher in hierarchy than the bot!");
                return;
            }
            if(ranks.Any(x => x.Id != role.Id))
            {
                await ReplyAsync("This role is not a rank!");
                return;
            }

            await _ranks.RemoveRankAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} is not longer a rank!");
        }

        [Command("add-rank", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task AddRank([Remainder]string name) // Remainder allows spaces
        {
            await Context.Channel.TriggerTypingAsync(); // adds typing indicator for the user to know that the bo is working on the solution

            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if(role == null)
            {
                await ReplyAsync("This role does not exist!");
                return;
            }

            if(role.Position > Context.Guild.CurrentUser.Hierarchy) // prevents bot from adding roles higher than its own
            {
                await ReplyAsync("That role is higher in hierarchy than the bot!");
                return;
            }

            if(ranks.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("This role is already a rank!");
                return;
            }

            await _ranks.AddRankAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} has been added to the ranks!");

        }

        [Command("autoroles", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AutoRoles()
        {
            await Context.Channel.TriggerTypingAsync(); // adds typing indicator for the user to know that the bot is working on the solution
            var autoRoles = await _autoRolesHelper.GetAutoRolesAsync(Context.Guild);

            if(autoRoles.Count == 0)
            {
                await ReplyAsync("This server does not yet have any auto roles!");
                return;
            }

            string description = "This message lists all available auto roles.\nIn order to add an auto role, you can use the name or ID of the auto role (if two or more auto roles have the same name)";
            foreach(var autoRole in autoRoles)
            {
                description += $"\n{autoRole.Mention} ({autoRole.Id})";
            }
            await ReplyAsync(description);
        }

        [Command("add-autorole", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task AddAutoRole([Remainder]string name) // Remainder allows spaces
        {
            await Context.Channel.TriggerTypingAsync(); // adds typing indicator for the user to know that the bot is working on the solution

            var autoRoles = await _autoRolesHelper.GetAutoRolesAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if(role == null)
            {
                await ReplyAsync("This role does not exist!");
                return;
            }

            if(role.Position > Context.Guild.CurrentUser.Hierarchy) // prevents bot from adding roles higher than its own
            {
                await ReplyAsync("That role is higher in hierarchy than the bot!");
                return;
            }

            if(autoRoles.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("This role is already an auto role!");
                return;
            }

            await _autoRoles.AddAutoRoleAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} has been added to the auto roles!");
        }

        [Command("delete-autorole", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task DeleteAutoRole([Remainder]string name) // Remainder allows spaces
        {
            await Context.Channel.TriggerTypingAsync(); // adds typing indicator for the user to know that the bot is working on the solution
            var autoRoles = await _autoRolesHelper.GetAutoRolesAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if(role == null)
            {
                await ReplyAsync("This role does not exist!");
                return;
            }

            if(role.Position > Context.Guild.CurrentUser.Hierarchy) // prevents bot from adding roles higher than its own
            {
                await ReplyAsync("That role is higher in hierarchy than the bot!");
                return;
            }

            if(autoRoles.Any(x => x.Id != role.Id))
            {
                await ReplyAsync("This role is not an auto role!");
                return;
            }

            await _autoRoles.RemoveAutoRoleAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} is no longer an auto role!");
        }
    }

}