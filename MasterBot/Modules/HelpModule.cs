using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Interactivity;
using Interactivity.Pagination;

namespace MasterBot.Modules
{
    public class HelpModule : ModuleBase
    {
        public readonly InteractivityService _interactivity;

        public HelpModule(InteractivityService inter)
        {
            _interactivity = inter;
        }

        [Command("help", RunMode = RunMode.Async)]
        public async Task PaginatorAsync()
        {
            var pages = new PageBuilder[] 
            {
                new PageBuilder().WithTitle("Configuration").WithFields(new EmbedFieldBuilder[] // CONFIG COMMANDS
                {
                    new EmbedFieldBuilder().WithName("`!prefix [prefix]`:\t**MANDATORY**").WithValue("Changes the prefix used on the server to call the bot commands. E.g. an admin would use `!command` by default, but after sending `!prefix .` the prefix changes to `.`, so one would use `.command`."),
                    new EmbedFieldBuilder().WithName("`!adminchannel [#channel || none]`:\t**MANDATORY**").WithValue("Sets the channel as the admin channel. It has to be set up for activity-check features to work. E.g. `!adminchannel` sets the channel this is used in as the admin channel; `!adminchannel channel` sets `channel` as the admin channel."),
                    new EmbedFieldBuilder().WithName("`!logging [true || false]`:\t**MANDATORY**").WithValue("Sets the logging option to the given value. Example: `!logging true` sets logging to true. Now will log all the bot activity to LogChannel."),
                    new EmbedFieldBuilder().WithName("`!logchannel [#logchannel]:`\t**MANDATORY**").WithValue(" Sets the log channel as the logging channel. If command is used while logging is false, sets it to true automatically. Example: `!logchannel #log-channel` will set `#log-channel` as the default channel. If the channel is not given, then sets the channel the command was issued as the logging channel."),
                    new EmbedFieldBuilder().WithName("`!ranks`:").WithValue("Displays all the ranks available on the server."), 
                    new EmbedFieldBuilder().WithName("`!delete-rank [rank]`:").WithValue("Deletes a specified rank from the server and the database. Use: `!delete-rank rank` removes `rank` from the database and deletes `rank` from all the users who previously had it."),
                    new EmbedFieldBuilder().WithName("`!add-rank [rank]`:").WithValue("Adds the specified rank to the server and the database. Use: `!add-rank rank` adds `rank` to the database and allows users to add it to their ranks."),
                    new EmbedFieldBuilder().WithName("`!autoroles`:").WithValue("Displays the list of the roles given to all users when they join the server."),
                    new EmbedFieldBuilder().WithName("`!delete-autorole [role]`:").WithValue("Deletes the specified autorole from the server. The role itself is not deleted but it is no longer given to users on server join. E.g. `!delete-autorole role`  will no longer add `role` to users who join the server after it is called."),
                    new EmbedFieldBuilder().WithName("`!add-autorole [role]`:").WithValue("Adds the specified role to the list of autoroles. After the role is added, it is given to users who join the server. E.g. `!add-autorole role` will add `role` to anyone who joins the server after this command is called.")
                }).WithText("All **MANDATORY** fields are necessary for proper bot function."),

                new PageBuilder().WithTitle("General").WithFields(new EmbedFieldBuilder[] // GENERAL COMMANDS
                {
                    new EmbedFieldBuilder().WithName("`!info`:").WithValue("Displays the information about the specified user in an embed: user avatar, user ID, discriminator (the numbers after the #), account creation date, server join date, assigned roles. If the user is not specified, displays the information about the caller."),
                    new EmbedFieldBuilder().WithName("`!server`:").WithValue("Displays the information about the server: server avatar, server creation date, number of people on the server, number of people online at the moment."),
                    new EmbedFieldBuilder().WithName("`!rank [rank]`:").WithValue("Adds (if the user does not already have the rank) or removes (if the user does have the rank) the specified rank from the callee. The argument could either be the rank name or the rank ID. Tip: ranks & roles should have identical names. Use: `!rank rank` will add `rank` to the user's ranks.")
                }),

                new PageBuilder().WithTitle("Moderation").WithFields(new EmbedFieldBuilder[] // MODERATION COMMANDS
                {
                    new EmbedFieldBuilder().WithName("`!purge [number of messages]`:").WithValue("Deletes a specified number of messages from the channel it was called in. Use: `!purge 50` will remove 50 messages."),
                }),

                new PageBuilder().WithTitle("LFG").WithFields(new EmbedFieldBuilder[] // LFG COMMANDS
                {
                    new EmbedFieldBuilder().WithName("`!lfg-register [Game] [Rating] [Region]`:").WithValue("Registers the user. The user has to provide some information, namely: GameName, Rating (could be = None) and Region."),
                    new EmbedFieldBuilder().WithName("`!lfg [Game] [Mode] [Lower rating boundary] [Upper rating boundary]`:").WithValue("Adds user to the LFG database, mentions all the users on the server who fit the rating requirement set by the user and are also looking for game."),
                    new EmbedFieldBuilder().WithName("`!lfg [Game][Mode] remove`:").WithValue("Removes the user from the lfg database for that mode.")
                }).WithText("Game Modes:\n `Casual`, `Ranked`, `Customs`, `Zombies`\n\nRegions:\n`EU`,`NA`, `Africa`, `SEA`, `Asia`, `ME`, `LA`, `RU`\n\nGames:\nUse full game names with no spaces or abbreviations."),
                
                new PageBuilder().WithTitle("Activity Check"), // ACTIVITY CHECK COMMANDS
                new PageBuilder().WithTitle("Fun").WithFields(new EmbedFieldBuilder[]
                {
                    new EmbedFieldBuilder().WithName("`!meme [subreddit=optional]`:\tALIAS `reddit`").WithValue("Sends a random post from the specified subreddit. If the subreddit is not specified, sends a random meme from /r/dankmemes. Use: `!reddit memes`, `!meme`, `!meme allthingsprotoss`, `!reddit`"),
                }),

                new PageBuilder().WithTitle("Music").WithFields(new EmbedFieldBuilder[]
                {
                    new EmbedFieldBuilder().WithName("`!join`:").WithValue("Makes the bot join the voice channel that the user is connected to."),
                    new EmbedFieldBuilder().WithName("`!disconnect`:").WithValue("Stops the music from playing and disconnects the bot from the voice channel."),
                    new EmbedFieldBuilder().WithName("`!play`:").WithValue("Searches YouTube for the specified keywords and plays the top result. If there is a track already playing, adds it to the queue."),
                    new EmbedFieldBuilder().WithName("`!skip`:").WithValue("If the track queue is not empty, skips ot the next track."),
                    new EmbedFieldBuilder().WithName("`!pause`:").WithValue("Pauses the current track."),
                    new EmbedFieldBuilder().WithName("`!resume`:").WithValue("Resumes the paused track, if there is one.")
                })
            };

            var paginator = new StaticPaginatorBuilder()
                .WithPages(pages)
                .WithFooter(PaginatorFooter.PageNumber | PaginatorFooter.Users)
                .WithEmotes(new Dictionary<IEmote, PaginatorAction>()
                {
                    [new Emoji("◀️")] = PaginatorAction.Backward,
                    [new Emoji("▶️")] = PaginatorAction.Forward
                })
                .Build();


                // var dm = await Context.User.SendMessageAsync("You called for help!");
                // await _interactivity.SendPaginatorAsync(paginator, dm.Channel, null, dm);
            
            
                // await Context.Channel.SendMessageAsync("User DM privacy restrictions prevent the bot from sending them messages.");
                await _interactivity.SendPaginatorAsync(paginator, Context.Channel);
            
            // return Interactivity.SendPaginatorAsync(paginator,
            //                                         Context.Channel,
            //                                         TimeSpan.FromMinutes(2));
            // return ReplyAsync(null, false, paginator);
            // return _interactivity.SendPaginatorAsync(paginator, Context.Channel);
        }
    }
}