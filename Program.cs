using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Finder.Bot.Modules;
using Finder.Database.DatabaseContexts;
using Finder.Database.Repositories;
using Finder.Bot.Handlers;
using Finder.Bot.Modules.Helpers;
using Finder.Database.Repositories.Bot;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Finder.Bot;

class Program {
    static void Main(string[] args) => RunAsync().GetAwaiter().GetResult();
    static async Task RunAsync() {
        IConfiguration configuration = new ConfigurationBuilder().AddJsonFile($"appsettings.json", false, true).Build();
        await using ServiceProvider services = ConfigureServices(configuration);
        DiscordShardedClient client = services.GetRequiredService<DiscordShardedClient>();
        InteractionService commands = services.GetRequiredService<InteractionService>();
        CommandHandler handler = services.GetRequiredService<CommandHandler>();
        await handler.Initialize();
        client.Log += LoggingService.LogAsync;
        commands.Log += LoggingService.LogAsync;
        CountdownTimer.StartTimer(client, services.GetRequiredService<CountdownRepository>());
        UnBanMuteTimer.StartTimer(client, services.GetRequiredService<UserLogsRepository>(), services.GetRequiredService<SettingsRepository>());
        client.ReactionAdded += TicTacToeModule.OnReactionAddedEvent;
        client.ReactionAdded += new ModerationModule(services.GetRequiredService<SettingsRepository>(), services.GetRequiredService<UserLogsRepository>()).OnReactionAddedEvent;
        client.ButtonExecuted += new PollModule(services.GetRequiredService<PollsRepository>()).OnButtonExecutedEvent;
        client.ButtonExecuted += new TicketingModule.TicketsModule(services.GetRequiredService<TicketsRepository>()).OnButtonExecutedEvent;
        client.MessageReceived += new LevelingModule(services.GetRequiredService<LevelingRepository>()).OnMessageReceivedEvent;
        await client.LoginAsync(TokenType.Bot, configuration["token"]);
        await client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }

    private static ServiceProvider ConfigureServices(IConfiguration configuration) {
        return new ServiceCollection()
            .AddDbContext<ApplicationContext>(options => options.UseNpgsql(configuration.GetConnectionString("Default")!, builder => builder.MigrationsAssembly("Finder.Database")), ServiceLifetime.Transient)
            .AddSingleton(configuration)
            .AddSingleton<DiscordShardedClient>()
            .AddSingleton<InteractionService>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<AddonsRepository>()
            .AddSingleton<CountdownRepository>()
            .AddSingleton<EconomyRepository>()
            .AddSingleton<LevelingRepository>()
            .AddSingleton<PollsRepository>()
            .AddSingleton<SettingsRepository>()
            .AddSingleton<TicketsRepository>()
            .AddSingleton<UserLogsRepository>()
            .AddSingleton<ItemInvRepository>()
            .BuildServiceProvider();
    }
}
