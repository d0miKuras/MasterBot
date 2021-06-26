using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Infrastructure;
using Discord.WebSocket;
using Discord.Commands;
using MasterBot.Utilities;
using Microsoft.Extensions.Logging;

namespace MasterBot.Modules
{
    public class LFGModule : ModuleBase
    {
        private readonly ILogger<LFGModule> _logger;
        private readonly LFGHelper _lfgHelper;
        private readonly Games _games;
        private readonly PlayerHelper _playerHelper;
        private readonly Players _players;
        private readonly LFGs _lfgs;

        public LFGModule(ILogger<LFGModule> logger, LFGHelper lfgHelper, Games games, PlayerHelper playerHelper, Players players, LFGs lfgs)
        {
            _logger = logger;
            _lfgHelper = lfgHelper;
            _games = games;
            _playerHelper = playerHelper;
            _players = players;
            _lfgs = lfgs;
        }

        [Command("addgame", RunMode = RunMode.Async)]
        public async Task AddGame([Remainder]string cmd) // adds a game to the database. Format: !addgame Name Shortname IntegerRankSystem(true or false), TODO:REMOVE LATER 
        {
            if (string.IsNullOrWhiteSpace(cmd)) 
            {
                await ReplyAsync("Please provide game details.");
                return;
            }

            var queries = cmd.Split(' ');
            bool.TryParse(queries[2], out bool intRanked);
            await _games.AddGameAsync(queries[0], queries[1], intRanked);
            
        }

        /// <summary>
        /// Registers the caller to the lfg database. Format: !lfg-register GameName Rank Region
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [Command("lfg-register", RunMode = RunMode.Async)]
        public async Task LfgRegister([Remainder]string cmd)
        {
            await Context.Channel.TriggerTypingAsync(); // adds typing indicator for the user to know that the bo is working on the solution

            if (string.IsNullOrWhiteSpace(cmd)) 
            {
                await ReplyAsync("Please provide game details.");
                return;
            }

            var queries = cmd.Split(' ');
            // for(int i = 0; i < queries.Length; i++)
            // {
            //     await ReplyAsync($"q[{i}] == {queries[i]}");
            // }
            var players = await _playerHelper.GetPlayersAsync(Context.Guild);

            var game = await _games.GetGameAsync(queries[0]);
            if(game == null)
            {
                await ReplyAsync("This game does not exist in the database!");
                return;
            }

            var existingPlayer = players.FirstOrDefault(x =>
                                                        (x.UserId == Context.User.Id &&
                                                        x.ServerId == Context.Guild.Id 
                                                        &&
                                                        (string.Equals(queries[0], x.Game.ShortName, StringComparison.CurrentCultureIgnoreCase) || string.Equals(queries[0], x.Game.Name, StringComparison.CurrentCultureIgnoreCase))
                                                        &&
                                                        string.Equals(x.Region.ToString(), queries[2], StringComparison.CurrentCultureIgnoreCase)
                                                        ));
            // await ReplyAsync($"existing player is null == {existingPlayer == null}");

            if(existingPlayer != null)
            {
                await ReplyAsync($"{Context.User.Mention} is already registered!");
                return;
            }
            else
            {
                await _players.AddPlayerAsync(Context.User.Id, Context.Guild.Id, queries[0], queries[1], (Region)Enum.Parse(typeof(Region), queries[2], true));
                await ReplyAsync($"{Context.User.Mention} registered:\nGame:\t{queries[0]}\nRegion:\t{queries[2]}\nRank:\t{queries[1]}");
            }
        }


        /// <summary>
        /// Creates a new row in the LFGs table. Mentions users already in the table. Format: !lfg Game Mode (optional)LowerBoundRank (optional)UpperBoundRank
        ///                                                                               or: !lfg Game Mode remove
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>            
        [Command("lfg", RunMode = RunMode.Async)]
        // TODO: remove unnecessary lists (fitting users might be replacable by a string += user.userId))
        // TODO: Only mention users whose lfg rating requirement include the caller
        public async Task AddToLfg([Remainder]string cmd) 
        {
            await Context.Channel.TriggerTypingAsync(); // adds typing indicator for the user to know that the bo is working on the solution

            if (string.IsNullOrWhiteSpace(cmd)) 
            {
                await ReplyAsync("Please provide game details.");
                return;
            }

            var queries = cmd.Split(' ');
            var game = await _games.GetGameAsync(queries[0]); // checks whether or not the game exists in the database and hence is accounted for in Rating
            if(game == null)
            {
                await ReplyAsync("This game is not in the database!");
                return;
            }

            if(!Enum.TryParse<Mode>(queries[1], true, out Mode mode))
            {
                await ReplyAsync("This mode does not exist! All possible modes are: Casual, Ranked, Customs, Zombies");
                return;
            }
            (int lowerBound, int upperBound) = (0, 0); // default values for Rating == None
            var caller = await _players.GetPlayerAsync(Context.Guild.Id, Context.User.Id);
            if(queries.Length > 2) // if the optional parameters are present, check if they are in database
            {
                if(string.Equals(queries[2], "remove")) // means queries[2] is remove
                {
                    if(caller == null)
                    {
                        await ReplyAsync("You are not on the list!");
                        return;
                    }

                    await _lfgs.RemoveLFGAsync(Context.Guild.Id, game, caller, mode);
                    await ReplyAsync($"Removed you from the lfg list for {game.Name} {mode}!");
                    return;
                }
                // if(!Enum.TryParse(queries[2], out Rating lowerRatingBound, true) || !Enum.TryParse(queries[3], out Rating upperRatingBound)) 
                if(!Enum.TryParse<Rating>(queries[2], true,  out Rating lowerRatingBound) || !Enum.TryParse<Rating>(queries[3], true, out Rating upperRatingBound)) 
                //TODO: need to figure out a way to utilize lowerRatingBound and upperRatingBound or remove them
                {
                    var ratingsAvailable = Enum.GetNames<Rating>();
                    string ratings = "The rating bounds mentioned are not in the database! All available rankings for all games:\n";
                    foreach(var item in ratingsAvailable)
                    {
                        ratings += $"{item}\n";
                    }
                    await ReplyAsync(ratings);
                    return;
                }
                
            }
            
            var players = await _lfgHelper.GetLFGsAsync(Context.Guild, queries[0], mode); // gets players who are looking for the same game as the caller
            
            

            List<IGuildUser> fittingUsers = new List<IGuildUser>();

            if(mode == Mode.Ranked) // if the caller is looking for ranked games, check the rating
            {
                // changing lowerBound and upperBound from default values
                lowerBound = (int)Enum.Parse(typeof(Rating), queries[2], true);
                upperBound = (int)Enum.Parse(typeof(Rating), queries[3], true);
                foreach(var player in players) // filters players who fit the rating requirement
                {
                    if(((int)player.Rank >= lowerBound
                    && (int)player.Rank <= upperBound))
                    {
                        IGuildUser user = await Context.Guild.GetUserAsync(player.UserId);
                        fittingUsers.Add(user);
                    }
                }
            }
            else // if the game mode is casual || customs || zombies
            {
                foreach(var player in players)
                {
                    IGuildUser user = await Context.Guild.GetUserAsync(player.UserId);
                        fittingUsers.Add(user);
                }
            }
            await _lfgs.AddLFGAsync(Context.Guild.Id, game, caller, mode, (Rating)lowerBound, (Rating)upperBound); // adding the new lfg created by the user to the database
            string mentions = $"{Context.User.Mention} is looking for game. These players fit your criteria and are also looking for a game:\n";

            foreach(var user in fittingUsers)
                mentions += $"{user.Mention} ";


            await ReplyAsync(mentions);
        }


    }
}