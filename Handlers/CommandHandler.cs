using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Finder.Bot.Handlers {
    public class CommandHandler {
        private readonly InteractionService commands;
        private readonly DiscordShardedClient client;
        private readonly IConfiguration config;
        private readonly IServiceProvider services;
        public CommandHandler(InteractionService _commands, DiscordShardedClient _client, IConfiguration _config, IServiceProvider _services) {
            commands = _commands;
            client = _client;
            config = _config;
            services = _services;
        }

        public async Task Initialize() {
            await commands.AddModulesAsync(Assembly.GetExecutingAssembly(), services);
            client.InteractionCreated += InteractionCreated;
            client.ButtonExecuted += ButtonExecuted;
            client.ShardReady += ShardReady;
            commands.SlashCommandExecuted += commandsSlashCommandExecuted;
            commands.AutocompleteHandlerExecuted += commandsAutocompleteHandlerExecuted;
        }

        private Task commandsAutocompleteHandlerExecuted(IAutocompleteHandler arg1, Discord.IInteractionContext arg2, IResult arg3) {
            return Task.CompletedTask;
        }

        private Task commandsSlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3) {
            return Task.CompletedTask;
        }

        private async Task ButtonExecuted(SocketMessageComponent arg) {
            await commands.ExecuteCommandAsync(new ShardedInteractionContext(client, arg), services);
        }

        private async Task ShardReady(DiscordSocketClient arg) {
            await RegisterCommands();
            client.ShardReady -= ShardReady;
        }

        private async Task InteractionCreated(SocketInteraction arg) {
            await commands.ExecuteCommandAsync(new ShardedInteractionContext(client, arg), services);
        }

        private async Task RegisterCommands() {
            #if DEBUG
                await commands.RegisterCommandsToGuildAsync(ulong.Parse(config["testGuild"]), true);
            #else
                await commands.RegisterCommandsGloballyAsync(true);
            #endif
        }
    }
}