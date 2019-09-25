using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using Discord;

namespace NMDBot.Modules
{
    public class SayModule : ModuleBase
    {
        [Command("say"), Summary("echos a message")]
        public async Task Say([Remainder, Summary("The text to echo")] string echo)
        {
            // Make sure this command came from a user and not the bot!
            var message = Context.Message as SocketUserMessage;
            if (message.Author.IsBot)
            {
                return;
            }

            await message.DeleteAsync();
            await ReplyAsync(echo);
        }
    }
}
