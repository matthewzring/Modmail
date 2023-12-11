using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Modmail.services
{
    public class Listener
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _configuration;

        public delegate Task AsyncListener<in TEventArgs>(TEventArgs args);
        public event AsyncListener<SocketMessage>? MessageReceived;

        public Listener(DiscordSocketClient client, IConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;
            client.MessageReceived += ClientOnMessageReceived;
        }

        private async Task ClientOnMessageReceived(SocketMessage arg)
        {
            if (MessageReceived is not null)
                _ = MessageReceived(arg);

            if (!arg.Author.IsBot) // ignore bot messages
            {
                if (arg.Channel is IPrivateChannel)
                {
                    // Handle the modmail received
                    await HandleModmailReceived(_client, arg);
                }
            }
        }

        public async Task HandleModmailReceived(DiscordSocketClient client, SocketMessage message)
        {
            // Basic checks
            SocketGuild guild = client.GetGuild(ulong.Parse(_configuration["GUILD_ID"]));
            if (guild == null)
                return;

            SocketUser author = message.Author;
            if (guild.GetUser(author.Id) == null)
                return;

            ICategoryChannel category = guild.GetCategoryChannel(ulong.Parse(_configuration["MODMAIL_CATEGORY_ID"]));
            if (category == null)
                return;

            // Check if there is a channel that has the user ID in the topic
            ITextChannel channel = guild.TextChannels.FirstOrDefault(x => x.Topic == $"User ID: {author.Id}" && x.CategoryId == category.Id);

            // If there is no channel, create one
            if (channel == null)
            {
                channel = await guild.CreateTextChannelAsync(author.Username.ToLower(), x =>
                {
                    x.CategoryId = category.Id;
                    x.Topic = $"User ID: {author.Id}";
                });

                // Send a greeting message to the user
                var greeting = new EmbedBuilder().WithTitle("Custom Greeting Message")
                                                 .WithColor(0xFF4400) // red
                                                 .WithDescription($"Thank you for contacting the {guild.Name} moderation team, someone will " +
                                                                  $"respond as soon as they are able to. In the meantime if you have not fully " +
                                                                  $"explained your issue, please do so now.")
                                                 .WithFooter($"{guild.Name} | {guild.Id} • {message.Timestamp.ToLocalTime().ToString("MM/dd/yyyy hh:mm tt")}");
                await author.SendMessageAsync(embed: greeting.Build());

                // Send a notification to the modmail
                await channel.SendMessageAsync("@here");
            }

            // Get the contents of the message
            string content = message.Content;
            string imageurl = message.Attachments?.FirstOrDefault()?.ProxyUrl;
            if (content is null && imageurl is null)
                return;

            // Send a confirmation message to the user
            var confirmation = new EmbedBuilder().WithTitle("Message Sent")
                                                 .WithColor(0x00FF00) // green
                                                 .WithDescription(content)
                                                 .WithFooter($"{guild.Name} | {guild.Id} • {message.Timestamp.ToLocalTime().ToString("MM/dd/yyyy hh:mm tt")}");
            if (imageurl is not null)
                confirmation.WithImageUrl(imageurl);
            await author.SendMessageAsync(embed: confirmation.Build());

            // Send the message to the modmail
            var modmail = new EmbedBuilder().WithAuthor(author.Username, author.GetAvatarUrl())
                                            .WithTitle("Message Received")
                                            .WithColor(0xFF4400) // red
                                            .WithDescription(content)
                                            .WithFooter($"{guild.Name} | {guild.Id} • {message.Timestamp.ToLocalTime().ToString("MM/dd/yyyy hh:mm tt")}");
            if (imageurl is not null)
                modmail.WithImageUrl(imageurl);
            await channel.SendMessageAsync(embed: modmail.Build());
        }
    }
}
