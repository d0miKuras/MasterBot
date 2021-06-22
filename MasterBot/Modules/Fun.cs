using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace MasterBot.Modules
{
    public class Fun : ModuleBase
    {
        [Command("meme")]
        [Alias("reddit")]
        public async Task Meme(string subreddit = null) // gets a random meme from reddit api and transforms it into an embed
        {
            var client = new HttpClient();
            var result = await client.GetStringAsync($"https://reddit.com/r/{subreddit ?? "dankmemes"}/random.json?limit=1"); // gets string form reddit api

            JArray arr = JArray.Parse(result); // parses into json
            JObject post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString()); // filters out metadata

            if(!result.StartsWith("["))
            {
                await Context.Channel.SendMessageAsync("This subreddit does not exist!");
                return;
            }
            var builder = new EmbedBuilder()
                .WithImageUrl(post["url"].ToString())
                .WithColor(new Color(new Random().Next(0, 255), new Random().Next(0, 255), new Random().Next(0, 255)))
                .WithTitle(post["title"].ToString())
                .WithUrl("https://reddit.com" + post["permalink"].ToString())
                .WithFooter($"🗨️ {post["num_comments"]} ⬆️ {post["ups"]}");
            
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }
    }
}