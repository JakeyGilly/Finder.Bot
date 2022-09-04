using Discord.Interactions;
using Discord.WebSocket;
using Finder.Bot.Repositories.Bot;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Finder.Bot.Handlers {
    public class CommandHandler {
        private readonly InteractionService commands;
        private readonly DiscordShardedClient client;
        private readonly IConfiguration config;
        private readonly IServiceProvider services;
        private readonly AddonsRepository addonsRepository;
        public CommandHandler(InteractionService _commands, DiscordShardedClient _client, IConfiguration _config, IServiceProvider _services, AddonsRepository _addonsRepository) {
            commands = _commands;
            client = _client;
            config = _config;
            services = _services;
            addonsRepository = _addonsRepository;
        }

        public async Task Initialize() {
            await commands.AddModulesAsync(Assembly.GetExecutingAssembly(), services);
            client.InteractionCreated += InteractionCreated;
            client.ButtonExecuted += ButtonExecuted;
            client.ShardReady += ShardReady;
            commands.SlashCommandExecuted += CommandsSlashCommandExecuted;
            commands.AutocompleteHandlerExecuted += CommandsAutocompleteHandlerExecuted;
        }

        private Task CommandsAutocompleteHandlerExecuted(IAutocompleteHandler arg1, Discord.IInteractionContext arg2, IResult arg3) {
            return Task.CompletedTask;
        }

        private Task CommandsSlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3) {
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
            foreach (var guild in client.Guilds) {
                await commands.RegisterCommandsToGuildAsync(guild.Id);
            }
        }
    }
}