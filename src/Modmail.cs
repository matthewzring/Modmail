using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modmail.services;

namespace Modmail
{
    public class Modmail
    {
        public DiscordSocketClient _client { get; }

        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _services;

        public Modmail()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMessageReactions | GatewayIntents.MessageContent | GatewayIntents.GuildMembers,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100
            });

            _services = new ServiceCollection()
                .AddSingleton(_configuration)
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton(new Listener(_client, _configuration))
                .BuildServiceProvider();
        }

        static void Main(string[] args)
            => new Modmail().RunAsync().GetAwaiter().GetResult();

        public async Task RunAsync()
        {
            var client = _services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;

            await _services.GetRequiredService<CommandHandlingService>().InitializeAsync();

            Enum.TryParse(_configuration["activity"], out ActivityType activityType);
            await client.SetGameAsync(_configuration["status"], type: activityType);

            await client.LoginAsync(TokenType.Bot, _configuration["token"]);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task LogAsync(LogMessage message)
            => Console.WriteLine(message.ToString());
    }
}
