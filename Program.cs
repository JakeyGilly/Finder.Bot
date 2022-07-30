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
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Finder.Bot;

class Program {
    static void Main(string[] args) => RunAsync().GetAwaiter().GetResult();
    static async Task RunAsync() {
        if (!File.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "appsettings.json")) {
            Appsettings appsettings = new Appsettings() { ConnectionStrings = new ConnectionStrings() };
            await using StreamWriter file = File.CreateText(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "appsettings.json");
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(file, appsettings);
        }
        await using ServiceProvider services = ConfigureServices();
        DiscordShardedClient client = services.GetRequiredService<DiscordShardedClient>();
        InteractionService commands = services.GetRequiredService<InteractionService>();
        IConfiguration config = services.GetRequiredService<IConfiguration>();
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
        await client.LoginAsync(TokenType.Bot, config["token"]);
        await client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }

    private static ServiceProvider ConfigureServices() {
        IConfiguration configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", false, true).Build();
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
