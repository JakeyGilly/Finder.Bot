using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Bot.Database;
using Microsoft.Extensions.DependencyInjection;
using Finder.Bot.Modules;
using Finder.Bot.Handlers;
using Finder.Bot.Modules.Addons;
using Finder.Bot.Modules.Helpers;
using Finder.Bot.Repositories;
using Microsoft.EntityFrameworkCore;
namespace Finder.Bot;

class Program {
    static void Main() => RunAsync().GetAwaiter().GetResult();
    static async Task RunAsync() {
        await using ServiceProvider services = ConfigureServices();
        DiscordShardedClient client = services.GetRequiredService<DiscordShardedClient>();
        InteractionService commands = services.GetRequiredService<InteractionService>();
        CommandHandler handler = services.GetRequiredService<CommandHandler>();
        await handler.Initialize();
        client.Log += LoggingService.LogAsync;
        commands.Log += LoggingService.LogAsync;
        CountdownTimer.StartTimer(client, services.GetRequiredService<IUnitOfWork>());
        UnBanMuteTimer.StartTimer(client, services.GetRequiredService<IUnitOfWork>());
        client.ReactionAdded += TicTacToeModule.OnReactionAddedEvent;
        client.ReactionAdded += new ModerationModule(services.GetRequiredService<IUnitOfWork>()).OnReactionAddedEvent;
        client.ButtonExecuted += new PollModule(services.GetRequiredService<IUnitOfWork>()).OnButtonExecutedEvent;
        client.ButtonExecuted += new TicketingModule.TicketsModule(services.GetRequiredService<IUnitOfWork>()).OnButtonExecutedEvent;
        client.MessageReceived += new LevelingModule(services.GetRequiredService<IUnitOfWork>()).OnMessageReceivedEvent;
        await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("BOT_TOKEN"));
        await client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }

    private static ServiceProvider ConfigureServices() {
        return new ServiceCollection()
            .AddDbContext<ApplicationContext>(options => options.UseNpgsql(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")!), ServiceLifetime.Transient)
            .AddSingleton<DiscordShardedClient>()
            .AddSingleton<InteractionService>()
            .AddSingleton<CommandHandler>()
            .AddTransient(typeof(IRepository<>), typeof(Repository<>))
            .AddTransient<IUnitOfWork, UnitOfWork>()
            .BuildServiceProvider();
    }
}
