using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Bot.Resources;

namespace Finder.Bot.Modules {
    public class AdminModule : InteractionModuleBase<ShardedInteractionContext> {
        //todo: fix everything
        [SlashCommand("purge", "Purge a number of messages", runMode: RunMode.Async)]
        public async Task PurgeCommand(int count) {
            var messages = await Context.Channel.GetMessagesAsync(count).FlattenAsync();
            try {
                await ((SocketTextChannel)Context.Channel).DeleteMessagesAsync(messages);
            } catch (Exception e) {
                await ReplyAsync(AdminLocale.AdminError_purge);
            }
            await RespondAsync(embed: new EmbedBuilder() {
                Title = "Purged",
                Color = Color.Orange,
                Footer = new EmbedFooterBuilder() {
                    Text = Main.EmbedFooter
                }
            }.Build());
        }

        [SlashCommand("slowmode", "Set the slowmode of a channel", runMode: RunMode.Async)]
        public async Task SlowmodeCommand(int seconds) {
            if (seconds < 0) {
                await ReplyAsync(AdminLocale.AdminError_slowmodeNegative);
                return;
            }
            await ((SocketTextChannel)Context.Channel).ModifyAsync(x => x.SlowModeInterval = seconds);
            await RespondAsync(embed: new EmbedBuilder() {
                Title = AdminLocale.AdminEmbedSlowmode_title,
                Color = Color.Orange,
                Fields = {
                    new EmbedFieldBuilder() {
                        Name = AdminLocale.AdminEmbedSlowmode_field0Name,
                        Value = Context.Channel.Name,
                        IsInline = true
                    },
                    new EmbedFieldBuilder() {
                        Name = AdminLocale.AdminEmbedSlowmode_field1Name,
                        Value = seconds.ToString(),
                        IsInline = true
                    },
                    new EmbedFieldBuilder() {
                        Name = AdminLocale.AdminEmbedSlowmode_field2Name,
                        Value = Context.User.Username,
                        IsInline = true
                    }
                },
                Footer = new EmbedFooterBuilder() {
                    Text = Main.EmbedFooter
                }
            }.Build());
        }

        [SlashCommand("lockdown", "Lockdown a channel", runMode: RunMode.Async)]
        public async Task LockdownCommand() {
            OverwritePermissions overwrite = new OverwritePermissions(sendMessages: PermValue.Deny);
            foreach (var role in Context.Guild.Roles) {
                if (!role.Permissions.Administrator) {
                    await ((SocketTextChannel)Context.Channel).AddPermissionOverwriteAsync(role, overwrite);
                }
            }
            await RespondAsync(embed: new EmbedBuilder() {
                Title = AdminLocale.AdminEmbedLockdown_title,
                Color = Color.Orange,
                Fields = {
                    new EmbedFieldBuilder() {
                        Name = AdminLocale.AdminEmbedLockdown_field0Name,
                        Value = Context.Channel.Name,
                        IsInline = true
                    },
                    new EmbedFieldBuilder() {
                        Name = AdminLocale.AdminEmbedLockdown_field1Name,
                        Value = Context.User.Username,
                        IsInline = true
                    }
                },
                Footer = new EmbedFooterBuilder() {
                    Text = Main.EmbedFooter
                }
            }.Build());
        }
    }
}