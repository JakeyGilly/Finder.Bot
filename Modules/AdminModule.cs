using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Finder.Bot.Modules
{
    // only admin can use this command
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public class AdminModule : InteractionModuleBase<ShardedInteractionContext>
    {
        [SlashCommand("purge", "Purge a number of messages", runMode: RunMode.Async)]
        public async Task PurgeCommand(int count)
        {

          // This can get 100 messages only
            //var messages = await Context.Channel.GetMessagesAsync(count).FlattenAsync();
            //try {
            //    await ((SocketTextChannel)Context.Channel).DeleteMessagesAsync(messages);
            //} catch (Exception e) {
            //    await ReplyAsync("An error occurred while purging messages.");
            //}


            int counter = 0;
            var messages = await Context.Channel.GetMessagesAsync(count + 1).FlattenAsync();
            do
            {
                var messageIds = messages.Where(a => a != messages.FirstOrDefault() && a.Timestamp > DateTime.UtcNow.AddDays(-6)).Select(a => a.Id);

                await (Context.Channel as ITextChannel).DeleteMessagesAsync(messageIds);

                counter += messages.Count();

                var latestMessage = messages.LastOrDefault();
                if (latestMessage != null)
                {
                    if (counter < count && latestMessage.Timestamp > DateTime.UtcNow.AddDays(-6))
                        messages = await Context.Channel.GetMessagesAsync(latestMessage, Direction.Before, 100).FlattenAsync();
                    else
                        messages = null;
                }
            } while (messages != null);

            await RespondAsync(embed: new EmbedBuilder
            {
                Title = "Purged",
                Footer = new EmbedFooterBuilder
                {
                    Text = $"FinderBot deleted {count - 1} messages succesfully."
                }
            }.Build());
        }

        [SlashCommand("slowmode", "Set the slowmode of a channel", runMode: RunMode.Async)]
        public async Task SlowmodeCommand(int seconds)
        {
            if (seconds < 0)
            {
                await ReplyAsync("The slowmode must be greater than or equal to 0.");
                return;
            }
            await ((SocketTextChannel)Context.Channel).ModifyAsync(x => x.SlowModeInterval = seconds);
            await RespondAsync(embed: new EmbedBuilder
            {
                Title = "Slowmode set",
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder {
                        Name = "Channel",
                        Value = Context.Channel.Name,
                        IsInline = true
                    },
                    new EmbedFieldBuilder {
                        Name = "Slowmode",
                        Value = seconds.ToString(),
                        IsInline = true
                    },
                    new EmbedFieldBuilder {
                        Name = "By user",
                        Value = Context.User.Username,
                        IsInline = true
                    }
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = "FinderBot"
                }
            }.Build());
        }

        [SlashCommand("lockdown", "Lockdown a channel", runMode: RunMode.Async)]
        public async Task LockdownCommand()
        {
            OverwritePermissions overwrite = new OverwritePermissions(sendMessages: PermValue.Deny);
            foreach (var role in Context.Guild.Roles)
            {
                if (!role.Permissions.Administrator)
                {
                    await ((SocketTextChannel)Context.Channel).AddPermissionOverwriteAsync(role, overwrite);
                }
            }
            await RespondAsync(embed: new EmbedBuilder
            {
                Title = "Channel locked down",
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder {
                        Name = "Channel",
                        Value = Context.Channel.Name,
                        IsInline = true
                    },
                    new EmbedFieldBuilder {
                        Name = "By user",
                        Value = Context.User.Username,
                        IsInline = true
                    }
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = "FinderBot"
                }
            }.Build());
        }
    }
}