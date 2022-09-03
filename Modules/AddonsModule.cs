using Discord;
using Discord.Interactions;
using Finder.Database.Repositories.Bot;

namespace Finder.Bot.Modules {
    [Group("addons", "Command For Managing Addons")]
    public class AddonsModule : InteractionModuleBase<ShardedInteractionContext> {
        private readonly AddonsRepository addonsRepository;
        public AddonsModule(AddonsRepository _addonsRepository) {
            addonsRepository = _addonsRepository;
        }

        [SlashCommand("list", "Lists the installed addons", runMode: RunMode.Async)]
        public async Task GetAddons() {
            var value = await addonsRepository.GetAddonsModelAsync(Context.Guild.Id);
            var embed = new EmbedBuilder {
                Title = "Addon list",
                Footer = new EmbedFooterBuilder {
                    Text = "FinderBot"
                }
            };
            if (value == null || value.Addons.Count == 0) {
                foreach (var addon in Enum.GetValues(typeof(Shared.Enums.Addons))) {
                    embed.AddField(addon.ToString(), "Not installed", false);
                }
            } else {
                foreach (var addon in Enum.GetValues(typeof(Shared.Enums.Addons))) {
                    embed.AddField(addon.ToString(), value.Addons.Keys.Contains(addon.ToString()) && value.Addons.First(x => x.Key == addon.ToString()).Value == "true" ? "Installed" : "Not Installed", false);
                }
            }
            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("install", "Installs an addon", runMode: RunMode.Async)]
        public async Task InstallAddon([Autocomplete(typeof(AddonsInstallAutocompleteHandler))] string addon) {
            if (!Enum.TryParse(addon, out Shared.Enums.Addons addonEnum)) {
                await RespondAsync("Error: Addon not found");
                return;
            }
            if (await addonsRepository.AddonEnabled(Context.Guild.Id, addon)) {
                await RespondAsync("Error: Addon already installed");
                return;
            }
            await addonsRepository.AddAddonAsync(Context.Guild.Id, addon, "true");
            await addonsRepository.SaveAsync();
            await RespondAsync(embed: new EmbedBuilder {
                Title = "Addon Installed",
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder {
                        Name = "Addon",
                        Value = addon
                    }
                },
                Footer = new EmbedFooterBuilder {
                    Text = "FinderBot"
                }
            }.Build());
        }

        [SlashCommand("uninstall", "Uninstalls an addon", runMode: RunMode.Async)]
        public async Task UninstallAddon([Autocomplete(typeof(AddonsUninstallAutocompleteHandler))] string addon) {
            if (!Enum.TryParse(addon, out Shared.Enums.Addons addonEnum)) {
                await RespondAsync("Error: Addon not found");
                return;
            }
            if (!await addonsRepository.AddonEnabled(Context.Guild.Id, addon)) {
                await RespondAsync("Error: Addon not installed");
                return;
            }
            await addonsRepository.RemoveAddonAsync(Context.Guild.Id, addon);
            await addonsRepository.SaveAsync();
            await RespondAsync(embed: new EmbedBuilder {
                Title = "Addon Uninstalled",
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder {
                        Name = "Addon",
                        Value = addon
                    }
                },
                Footer = new EmbedFooterBuilder {
                    Text = "FinderBot"
                }
            }.Build());
        }
    }
    
    public class AddonsInstallAutocompleteHandler : AutocompleteHandler {
        private readonly AddonsRepository addonsRepository;
        public AddonsInstallAutocompleteHandler(AddonsRepository _addonsRepository) {
            addonsRepository = _addonsRepository;
        }
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {
            List<AutocompleteResult> results = new List<AutocompleteResult>();
            foreach (var addon in Enum.GetValues(typeof(Shared.Enums.Addons))) {
                if (!await addonsRepository.AddonEnabled(context.Guild.Id, addon.ToString())) {
                    results.Add(new AutocompleteResult(addon.ToString(), addon.ToString()));
                }
            }
            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }
    
    public class AddonsUninstallAutocompleteHandler : AutocompleteHandler {
        private readonly AddonsRepository addonsRepository;
        public AddonsUninstallAutocompleteHandler(AddonsRepository _addonsRepository) {
            addonsRepository = _addonsRepository;
        }
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {
            List<AutocompleteResult> results = new List<AutocompleteResult>();
            foreach (var addon in Enum.GetValues(typeof(Shared.Enums.Addons))) {
                if (await addonsRepository.AddonEnabled(context.Guild.Id, addon.ToString())) {
                    results.Add(new AutocompleteResult(addon.ToString(), addon.ToString()));
                }
            }
            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }
}