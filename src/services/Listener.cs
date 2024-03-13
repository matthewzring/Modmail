using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Modmail.logging;
using Modmail.utils;

namespace Modmail.services;

public class Listener
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _configuration;

    public delegate Task AsyncListener<in TEventArgs>(TEventArgs args);
    public event AsyncListener<SocketMessage>? MessageReceived;

    private const string OPEN = "\uD83D\uDCEC"; // 📬

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
        SocketGuild guild = client.GetGuild(ulong.Parse(_configuration["guild"]));
        if (guild == null)
            return;

        SocketUser author = message.Author;
        if (guild.GetUser(author.Id) == null)
            return;

        ICategoryChannel category = guild.GetCategoryChannel(ulong.Parse(_configuration["modmail_category"]));
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
            EmbedBuilder greeting = new EmbedBuilder().WithTitle("Custom Greeting Message")
                                             .WithColor(0xFF4400) // red
                                             .WithDescription($"Thank you for contacting the {guild.Name} moderation team, someone will " +
                                                              $"respond as soon as they are able to. In the meantime if you have not fully " +
                                                              $"explained your issue, please do so now.")
                                             .WithFooter(LogUtil.FooterFormat(guild.Name, guild.Id, message.Timestamp));
            await author.SendMessageAsync(embed: greeting.Build());

            // Send a notification to the modmail
            await channel.SendMessageAsync("@here");

            // Log the ticket creation
            ITextChannel tc = guild.GetTextChannel(ulong.Parse(_configuration["log_channel"]));
            if (tc == null)
                return;
            DateTimeOffset now = DateTimeOffset.UtcNow;
            await Logger.Log(now, tc, OPEN, $"New ticket opened by {FormatUtil.formatFullUser(author)}", null);
        }

        // Get the contents of the message
        string content = message.Content;
        string imageurl = message.Attachments?.FirstOrDefault()?.ProxyUrl;
        if (content is null && imageurl is null)
            return;

        // Send a confirmation message to the user
        EmbedBuilder confirmation = new EmbedBuilder().WithTitle("Message Sent")
                                             .WithColor(0x00FF00) // green
                                             .WithDescription(content)
                                             .WithFooter(LogUtil.FooterFormat(guild.Name, guild.Id, message.Timestamp));
        if (imageurl is not null)
            confirmation.WithImageUrl(imageurl);
        await author.SendMessageAsync(embed: confirmation.Build());

        // Send the message to the modmail
        EmbedBuilder modmail = new EmbedBuilder().WithAuthor(author.Username, author.GetAvatarUrl())
                                        .WithTitle("Message Received")
                                        .WithColor(0xFF4400) // red
                                        .WithDescription(content)
                                        .WithFooter(LogUtil.FooterFormat(guild.Name, guild.Id, message.Timestamp));
        if (imageurl is not null)
            modmail.WithImageUrl(imageurl);
        await channel.SendMessageAsync(embed: modmail.Build());
    }
}
