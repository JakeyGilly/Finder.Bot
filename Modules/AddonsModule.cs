using Discord;
using Discord.Interactions;
using Finder.Database.Repositories;
using Finder.Bot.Resources;
using Finder.Database.Repositories.Bot;
using Finder.Shared.Enums;

namespace Finder.Bot.Modules {
    
    [Group("addons", "Command For Managing Addons")]
    //todo: make this work
    public class AddonsModule : InteractionModuleBase<ShardedInteractionContext> {
        private readonly AddonsRepository addonsRepository;
        public AddonsModule(AddonsRepository _addonsRepository) {
            addonsRepository = _addonsRepository;
        }

        [SlashCommand("list", "Lists the installed addons", runMode: RunMode.Async)]
        public async Task GetAddons() {
            var value = await addonsRepository.GetAddonsAsync(Context.Guild.Id);
            var embed = new EmbedBuilder() {
                Title = AddonsLocale.AddonsEmbedList_title,
                Color = Color.Orange,
                Footer = new EmbedFooterBuilder() {
                    Text = Main.EmbedFooter
                }
            };
            if (value.Addons.Any()) {
                foreach (var addon in value.Addons) {
                    embed.AddField(name: addon.ToString(), value: value.Addons.Contains(addon) ? AddonsLocale.AddonsInstalled : AddonsLocale.AddonsNotInstalled, inline: false);
                }
            } else {
                foreach (var addon in value.Addons) {
                    embed.AddField(name: addon.ToString(), value: AddonsLocale.AddonsNotInstalled, inline: false);
                }
            }
            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("install", "Installs an addon", runMode: RunMode.Async)]
        public async Task InstallAddon(Addons addon) {
            var value = await addonsRepository.GetAddonsAsync(Context.Guild.Id);
            if (value.Addons.Contains(addon)) {
                await RespondAsync(AddonsLocale.AddonsError_alreadyInstalled);
            } else {
                await addonsRepository.AddAddonAsync(Context.Guild.Id, addon);
                await addonsRepository.SaveAsync();
                await RespondAsync(embed: new EmbedBuilder() {
                    Title = AddonsLocale.AddonsEmbedInstall_title,
                    Color = Color.Orange,
                    Fields = new List<EmbedFieldBuilder>() {
                        new EmbedFieldBuilder() {
                            Name = AddonsLocale.AddonsEmbed_fieldAddonName,
                            Value = string.Format(AddonsLocale.AddonsEmbed_fieldAddonValue, addon.ToString()),
                        }
                    },
                    Footer = new EmbedFooterBuilder() {
                        Text = Main.EmbedFooter
                    }
                }.Build());
                await RespondAsync("Addon installed");
            }
        }

        [SlashCommand("uninstall", "Uninstalls an addon", runMode: RunMode.Async)]
        public async Task UninstallAddon(Addons addon) {
            var value = await addonsRepository.GetAddonsAsync(Context.Guild.Id);
            if (!value.Addons.Contains(addon)) {
                await RespondAsync(AddonsLocale.AddonsError_notInstalled);
            } else {
                await addonsRepository.RemoveAddonAsync(Context.Guild.Id, addon);
                await addonsRepository.SaveAsync();
                await RespondAsync(embed: new EmbedBuilder() {
                    Title = AddonsLocale.AddonsEmbedUninstall_title,
                    Color = Color.Orange,
                    Fields = new List<EmbedFieldBuilder>() {
                        new EmbedFieldBuilder() {
                            Name = AddonsLocale.AddonsEmbed_fieldAddonName,
                            Value = string.Format(AddonsLocale.AddonsEmbed_fieldAddonValue, addon.ToString()),
                        }
                    },
                    Footer = new EmbedFooterBuilder() {
                        Text = Main.EmbedFooter
                    }
                }.Build());
                await RespondAsync("Addon uninstalled");
            }
        }
    }
}
