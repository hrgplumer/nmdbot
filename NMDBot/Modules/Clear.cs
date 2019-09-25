using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;

namespace NMDBot.Modules
{
    public class ClearModule : ModuleBase
    {
        private const int ClearLimit = 50;

        [Command("clear"), Summary("Clears the last X messages.")]
        public async Task Clear([Remainder, Summary("The number of messages to clear.")] int delete = 0)
        {
            var bot = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);

            // Check if we have proper permissions
            if (!bot.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("I'm not allowed to manage messages!");
                return;
            }

            // Check if the calling user has sufficient permissions
            var nomad = await Context.Guild.GetUserAsync(Context.User.Id);
            if (!nomad.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("Fuck off! You aren't allowed to manage messages!");
                return;
            }

            // Check that the user supplied the delete param
            if (delete == 0)
            {
                await Context.Channel.SendMessageAsync("Tell me how many messages to delete! Usage: !clear [amount]");
            }

            // Check that we are not over the message limit
            if (delete > ClearLimit)
            {
                await Context.Channel.SendMessageAsync($"Limit is {ClearLimit} messages.");
                return;
            }

            var deletedMessages = await Context.Channel.GetMessagesAsync(delete + 1).Flatten();
            await Context.Channel.DeleteMessagesAsync(deletedMessages);
        }
    }
}
