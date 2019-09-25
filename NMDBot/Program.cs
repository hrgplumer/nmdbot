using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Text.RegularExpressions;
//using NLog;

namespace NMDBot
{
    public class Program
    {
        private DiscordSocketClient _bot { get; set; }
        private CommandService _commands { get; set; }

        public static void Main(string[] args)
            => new Program().Start().GetAwaiter().GetResult();

        /// <summary>
        /// Entry point for the bot
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            _bot = new DiscordSocketClient();
            _commands = new CommandService();

            await LoadCommands();

            try
            {
                await _bot.LoginAsync(TokenType.Bot, Constants.Token.BotToken);
                await _bot.StartAsync();

                // Set game
                await _bot.SetGameAsync("with Light's bodypillows");

                // Await indefinitely so we stay connected.
                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                await Bot_Log(new LogMessage(LogSeverity.Error, "Start()",e.Message, e));
            }
        }

        /// <summary>
        /// Loads event handlers and Discord commands. Any classes that extend ModuleBase will be loaded here.
        /// </summary>
        /// <returns></returns>
        private async Task LoadCommands()
        {
            _bot.Log += Bot_Log;
            _bot.MessageReceived += HandleCommand;
            _bot.UserJoined += UserJoined;
            _bot.UserLeft += UserLeft;
            _bot.UserBanned += UserBanned;

            // Load all commands in this assembly.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        #region Event Handlers

        /// <summary>
        /// Logging event handler
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private Task Bot_Log(LogMessage message)
        {
            var cc = Console.ForegroundColor;
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message}");
            Console.ForegroundColor = cc;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles commands. Fired on each message send
        /// </summary>
        /// <param name="messageParam"></param>
        /// <returns></returns>
        private async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null)
            {
                return; // Don't process if this is a system message
            }

            // Don't process if this is from a bot
            if (message.Author.IsBot)
            {
                return;
            }

            // Is this message a command, or was the bot mentioned?
            var argPos = 0;
            if (!message.HasCharPrefix(Constants.BotConstants.CommandPrefix, ref argPos) || message.HasMentionPrefix(_bot.CurrentUser, ref argPos))
            {
                return;
            }

            var context = new CommandContext(_bot, message);

            // Execute the command
            var result = await _commands.ExecuteAsync(context, argPos);

            // Return the error if something went wrong.
            if (!result.IsSuccess)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        private async Task UserJoined(SocketGuildUser user)
        {
            // Announce new user in general and ping spiritual leader
            var guildId = user.Guild.Id;
            var role = user.Guild.Roles.Where(r => r.Name.ToUpper() == Constants.Roles.SpiritualLeader).FirstOrDefault();
            var generalChannel = GetChannel(_bot.GetGuild(guildId), "sorting");

            if (role == null)
            {
                await generalChannel.SendMessageAsync(user.Mention + " has joined the bum rush.");
            }
            else
            {
                await generalChannel.SendMessageAsync(role.Mention + ", " + user.Mention + " has joined the bum rush.");
            }
            
        }

        private async Task UserLeft(SocketGuildUser user)
        {
            // Announce user left in general
            var guildId = user.Guild.Id;
            var generalChannel = GetChannel(_bot.GetGuild(guildId), "sorting");
            await generalChannel.SendMessageAsync(user.Username + " became dead.");
        }

        private async Task UserBanned(SocketUser user, SocketGuild guild)
        {
            // Announce user banned in general
            var generalChannel = GetChannel(guild, "general");
            await generalChannel.SendMessageAsync("Fs in chat for " + user.Username + ", they're gone for good.");
        }

        #endregion

        #region Local Methods

        /// <summary>
        /// Checks for any REEEEing or o7s in chat
        /// </summary>
        /// <param name="message"></param>
        private static async void CheckForChatEvent(SocketUserMessage message)
        {
            // If the message was sent by a bot, disregard
            if (message.Author.IsBot)
            {
                return;
            }

            // Check for oh sevens, commander
            if (CheckForOhSeven(message)) await message.Channel.SendMessageAsync("o7 CMDR!");
            // Check for any rees in chat
            if (CheckForRee(message)) await message.Channel.SendMessageAsync(GetVariableRee());
            // Check for roblox
            if (CheckForRoblox(message)) await message.Channel.SendMessageAsync(GetVariableRee());
            // Check for oof
            //if (CheckForOof(message)) await message.Channel.SendMessageAsync("oof");
        }

        private static bool CheckForOhSeven(SocketUserMessage message)
        {
            var ohSevenRegex = "o7";
            var match = Regex.IsMatch(message.Content, ohSevenRegex);
            return match;
        }

        private static bool CheckForRee(SocketUserMessage message)
        {
            // This regex matches any ree with 2 or more Es, regardless of case.
            // It checks if the ree is its own word, to avoid false positives on words like agree.
            var reeRegex = @"\b((?i)re[e]+|RE[E]+)\w*";
            var match = Regex.IsMatch(message.Content, reeRegex);
            return match;
        }

        private static bool CheckForRoblox(SocketUserMessage message)
        {
            var robloxRegex = @"\b((?i)roblox)\w*";
            var match = Regex.IsMatch(message.Content, robloxRegex);
            return match;
        }

        private static bool CheckForOof(SocketUserMessage message)
        {
            var oofRegex = @"\b((?i)oof)\w*";
            var match = Regex.IsMatch(message.Content, oofRegex);
            return match;
        }

        private static SocketTextChannel GetChannel(SocketGuild guild, string channelName)
        {
            var channel = guild.TextChannels.FirstOrDefault(c => c.Name.ToUpper() == channelName.ToUpper());
            return channel;
        }

        private static string GetVariableRee()
        {
            var builder = new StringBuilder();
            builder.Append("REEEEEEEEEEEEEEEEEEEEEE");

            var random = new Random();
            var numberOfEs = random.Next(10);
            for (int i = 0; i < numberOfEs; i++)
            {
                builder.Append("E");
            }

            return builder.ToString();
        }

        #endregion
    }
}
