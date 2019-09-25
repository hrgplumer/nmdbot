using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;


namespace NMDBot.Modules
{
    public class ResetNicknamesModule : ModuleBase
    {
        [Command("resetnicknames"), Summary("Resets all user nicknames on this server")]
        public async Task ResetNicknames()
        {
            // get bot
            var bot = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);

            // Check if we have proper permissions
            if (!bot.GetPermissions(Context.Channel as ITextChannel).ManagePermissions)
            {
                await Context.Channel.SendMessageAsync("I'm not allowed to manage users!");
                return;
            }

            // Check if the calling user has sufficient permissions
            var nomad = await Context.Guild.GetUserAsync(Context.User.Id);
            if (!nomad.GetPermissions(Context.Channel as ITextChannel).ManagePermissions)
            {
                await Context.Channel.SendMessageAsync("Fuck off! You aren't allowed to do this!");
                return;
            }

            var users = await Context.Guild.GetUsersAsync();
            foreach (var user in users)
            {
                await user.ModifyAsync(u => u.Nickname = null);
            }
        }
    }
}
