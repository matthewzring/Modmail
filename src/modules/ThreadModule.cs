using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Modmail.logging;
using Modmail.utils;

namespace Modmail.modules
{
    public class ThreadModule : ModuleBase
    {
        private readonly IConfiguration _configuration;

        private const string CLOSE = "\uD83D\uDCEA"; // 📪
        private const string VIEW = "\uD83D\uDCC4"; // 📄
        private const string DOWNLOAD = "\uD83D\uDCE9"; // 📩

        public ThreadModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // reply
        [Command("r")]
        public async Task ReplyAsync([Remainder] string message)
        {
            // Check if we are currently in a channel in the modmail category
            ITextChannel channel = Context.Channel as ITextChannel;
            if (channel.CategoryId != ulong.Parse(_configuration["MODMAIL_CATEGORY_ID"]))
                return;

            // Get the user ID from the channel topic
            ulong userId = ulong.Parse(channel.Topic.Split(" ")[2]);

            // Generate a timestamp
            DateTimeOffset now = DateTimeOffset.Now;

            // Get the user
            SocketUser user = (SocketUser)await Context.Client.GetUserAsync(userId).ConfigureAwait(false);

            // Generate the anonymous reply embed 
            EmbedBuilder response = new EmbedBuilder().WithAuthor(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl())
                                             .WithTitle("Message Received")
                                             .WithColor(0xFF4400) // red
                                             .WithDescription(message)
                                             .WithFooter(LogUtil.FooterFormat(Context.Guild.Name, Context.Guild.Id, now));

            // Build the confirmation message
            EmbedBuilder confirmation = new EmbedBuilder().WithAuthor(Context.User.Username, Context.User.GetAvatarUrl())
                                                 .WithTitle("Message Sent")
                                                 .WithColor(0x00FF00) // green
                                                 .WithDescription(message)
                                                 .WithFooter(LogUtil.FooterFormat(Context.Guild.Name, Context.Guild.Id, now));

            try
            {
                // Send the message to the user
                await user.SendMessageAsync(embed: response.Build());

                // Delete the reply message
                await Context.Message.DeleteAsync();

                // Send the confirmation message
                await ReplyAsync(embed: confirmation.Build());
            }
            catch (Exception e) 
            {
                await ReplyAsync("An error occured when sending the reply (is the user blocking direct messages)?");
            }
        }

        // close
        [Command("close")]
        public async Task CloseAsync()
        {
            // Check if we are currently in a channel in the modmail category
            ITextChannel channel = Context.Channel as ITextChannel;
            if (channel.CategoryId != ulong.Parse(_configuration["MODMAIL_CATEGORY_ID"]))
                return;

            // Get the user ID from the channel topic
            ulong userId = ulong.Parse(channel.Topic.Split(" ")[2]);

            // Generate a timestamp
            DateTimeOffset now = DateTimeOffset.Now;

            // Get the user
            SocketUser user = (SocketUser)await Context.Client.GetUserAsync(userId).ConfigureAwait(false);

            // Build the notification
            EmbedBuilder notification = new EmbedBuilder().WithTitle("Thread Closed")
                                                  .WithColor(0xFF4400) // red
                                                  .WithDescription("This thread has been closed. Replying will create a new thread.")
                                                  .WithFooter(LogUtil.FooterFormat(Context.Guild.Name, Context.Guild.Id, now));

            try
            {
                // Notify the user
                await user.SendMessageAsync(embed: notification.Build());

                // Delete the channel
                await channel.DeleteAsync();

                // Log the ticket closure
                ITextChannel tc = await Context.Guild.GetTextChannelAsync(ulong.Parse(_configuration["LOG_CHANNEL_ID"]));
                if (tc == null)
                    return;
                
                /*
                Color botHighestRoleColor = Color.Default;
                if (Context.Client.CurrentUser is SocketGuildUser guildUser)
                {
                    IEnumerable<SocketRole> filterOutDefault = guildUser.Roles.Where(r => r.Color != Color.Default);
                    if (!filterOutDefault.Count().Equals(0))
                        botHighestRoleColor = filterOutDefault.MaxBy(r => r.Position).Color;
                }
                string view = "https://example.com";
                string download = "https://example.com";
                EmbedBuilder log = new EmbedBuilder().WithColor(botHighestRoleColor)
                                                    .WithDescription($"[`{VIEW} View`]({view})  |  [`{DOWNLOAD} Download`]({download})");
                */

                await Logger.Log(now, tc, CLOSE, $"Ticket by {FormatUtil.formatFullUser(user)} was closed", null);
            }
            catch (Exception e)
            {
                await ReplyAsync("An error occured when closing the thread.");
            }
        }
    }
}
