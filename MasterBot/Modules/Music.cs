using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Victoria;
using Victoria.Enums;

namespace MasterBot.Modules
{
    public class Music : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public Music(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
        }
        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinAsync() {
            if (_lavaNode.HasPlayer(Context.Guild)) {
                await ReplyAsync("I'm already connected to a voice channel!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null) {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            try {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
            }
            catch (Exception exception) {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("disconnect", RunMode = RunMode.Async)]
        public async Task Disconnect()
        {
            var voiceState = Context.User as IVoiceState;
            if(voiceState.VoiceChannel == null)
            {
                await ReplyAsync("I am not connected to a voice channel!");
                return;
            }

            try
            {
                await ReplyAsync($"Disconneted from {voiceState.VoiceChannel.Name}!");
                await _lavaNode.LeaveAsync(Context.Guild.CurrentUser.VoiceChannel);
            }
            catch(Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }
        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayAsync([Remainder] string searchQuery) {
            if (string.IsNullOrWhiteSpace(searchQuery)) {
                await ReplyAsync("Please provide search terms.");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild)) {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            var queries = searchQuery.Split(' ');
            foreach (var query in queries) {
                var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
                if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                    searchResponse.LoadStatus == LoadStatus.NoMatches) {
                    await ReplyAsync($"I wasn't able to find anything for `{query}`.");
                    return;
                }

                var player = _lavaNode.GetPlayer(Context.Guild);

                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused) {
                    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name)) {
                        foreach (var track in searchResponse.Tracks) {
                            player.Queue.Enqueue(track);
                        }

                        await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                    }
                    else {
                        var track = searchResponse.Tracks[0];
                        player.Queue.Enqueue(track);
                        await ReplyAsync($"Enqueued: {track.Title}");
                    }
                }
                else {
                    var track = searchResponse.Tracks[0];

                    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name)) {
                        for (var i = 0; i < searchResponse.Tracks.Count; i++) {
                            if (i == 0) {
                                await player.PlayAsync(track);
                                await ReplyAsync($"Now Playing: {track.Title}");
                            }
                            else {
                                player.Queue.Enqueue(searchResponse.Tracks[i]);
                            }
                        }

                        await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                    }
                    else {
                        await player.PlayAsync(track);
                        await ReplyAsync($"Now Playing: {track.Title}");
                    }
                }
            }
        }
        [Command("skip", RunMode = RunMode.Async)]
        public async Task Skip()
        {
            var player = _lavaNode.GetPlayer(Context.Guild);
            var passedCheckState = CheckState("skip", player);

            if(!passedCheckState.Result)
                return;
            await player.SkipAsync();
            await ReplyAsync($"Skipped! Now playing **{player.Track.Title}**!");

        }
        [Command("pause", RunMode = RunMode.Async)]
        public async Task Pause()
        {
            
            var player = _lavaNode.GetPlayer(Context.Guild);
            var passedCheckState = CheckState("pause", player);

            if(!passedCheckState.Result)
                return;
            await player.PauseAsync();
            await ReplyAsync("Paused the music.");
        } 

        [Command("resume", RunMode = RunMode.Async)]
        public async Task Resume()
        {
            var player = _lavaNode.GetPlayer(Context.Guild);
            var passedCheckState = CheckState("resume", player);

            if(!passedCheckState.Result)
                return;
            await player.ResumeAsync();
            await ReplyAsync("Resumed the track.");
        }

        

        private async Task<bool> CheckState(string cmd, LavaPlayer player)
        {
            var voiceState = Context.User as IVoiceState;
            if(voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return false;
            }
            if (!_lavaNode.HasPlayer(Context.Guild)) {
                await ReplyAsync("I'm not connected to a voice channel!");
                return false;
            }

            switch(cmd)
            {
                case "skip":
                    if(voiceState.VoiceChannel != player.VoiceChannel)
                    {
                        await ReplyAsync("You must be in the same channel as me!");
                        return false;
                    }
                    if(player.Queue.Count == 0)
                    {
                        await ReplyAsync("There are no more songs in the queue!");
                        return false;
                    }
                break;

                case "pause":
                if(voiceState.VoiceChannel != player.VoiceChannel)
                {
                    await ReplyAsync("You must be in the same channel as me!");
                    return false;
                }
                if(player.PlayerState == PlayerState.Paused || player.PlayerState == PlayerState.Stopped)
                {
                    await ReplyAsync("The music is already paused!");
                    return false;
                }
                break;

                case "resume":
                if(voiceState.VoiceChannel != player.VoiceChannel)
                {
                    await ReplyAsync("You must be in the same channel as me!");
                    return false;
                }
                if(player.PlayerState == PlayerState.Playing)
                {
                    await ReplyAsync("The music is already playing!");
                    return false;
                }
                break;
            }
            return true;
        }
    }
}