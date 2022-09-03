using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Bot.Resources;
using Finder.Database.Models.Bot;
using Finder.Database.Repositories.Bot;
namespace Finder.Bot.Modules.Addons {
    [DontAutoRegister]
    public class TicketingModule {
        [Group("tickets", "Command For Managing Tickets")]
        public class TicketsModule : InteractionModuleBase<ShardedInteractionContext> {
            ulong _closeConfirmId;
            private readonly TicketsRepository ticketsRepository;
            private readonly AddonsRepository addonsRepository;
            public TicketsModule(TicketsRepository _ticketsRepository, AddonsRepository _addonsRepository) {
                ticketsRepository = _ticketsRepository;
                addonsRepository = _addonsRepository;
            }
            [SlashCommand("create", "Creates a ticket", runMode: RunMode.Async)]
            public async Task CreateTicket(string? name = null) {
                if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Ticketing")) {
                    await RespondAsync(embed: new EmbedBuilder {
                        Title = "Ticketing",
                        Description = "This addon is disabled on this server.",
                        Color = Color.Red,
                        Fields = new List<EmbedFieldBuilder> {
                            new EmbedFieldBuilder() {
                                Name = "Enable",
                                Value = "Use `/addons install Ticketing` to enable this addon."
                            }
                        }
                    }.Build());
                    return;
                }
                if (name == null) {
                    await RespondAsync(TicketsLocale.TicketsError_noName);
                    return;
                }
                if (name.Length > 32) {
                    await RespondAsync(TicketsLocale.TicketsError_nameTooLong);
                    return;
                }
                var supportChannel = await Context.Guild.CreateTextChannelAsync(TicketsLocale.TicketsPrefix);
                await supportChannel.AddPermissionOverwriteAsync(Context.User, new OverwritePermissions(
                    addReactions: PermValue.Allow,
                    attachFiles: PermValue.Allow,
                    embedLinks: PermValue.Allow,
                    readMessageHistory: PermValue.Allow,
                    sendMessages: PermValue.Allow,
                    viewChannel: PermValue.Allow,
                    useApplicationCommands: PermValue.Allow
                ));
                await supportChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(
                    readMessageHistory: PermValue.Deny,
                    sendMessages: PermValue.Deny,
                    viewChannel: PermValue.Deny
                ));
                var components = new ComponentBuilder()
                    .WithButton(TicketsLocale.TicketsButton_closeBtn, "close")
                    .WithButton(TicketsLocale.TicketsButton_claimBtn, "claim")
                    .Build();
                var message = await supportChannel.SendMessageAsync(embed: new EmbedBuilder {
                    Title = TicketsLocale.TicketsEmbed_title,
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = name,
                            Value = string.Format(TicketsLocale.TicketsEmbed_fieldValue, Context.User.Username)
                        }
                    },
                    Color = Color.Green
                }.Build(), components: components);
                await ticketsRepository.AddTicketAsync(Context.Guild.Id, supportChannel.Id, message.Id, new List<long?> { (long)Context.User.Id }, name, new List<long>());
                await ticketsRepository.SaveAsync();
                await RespondAsync(embed: new EmbedBuilder {
                    Title = TicketsLocale.TicketsEmbedCreated_title,
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = string.Format(TicketsLocale.TicketsEmbedCreated_fieldValue),
                            Value = supportChannel.Mention
                        }
                    },
                    Color = Color.Green
                }.Build());
            }

            [SlashCommand("close", "Closes a ticket", runMode: RunMode.Async)]
            public async Task CloseTicket() {
                if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Ticketing")) {
                    await RespondAsync(embed: new EmbedBuilder {
                        Title = "Ticketing",
                        Description = "This addon is disabled on this server.",
                        Color = Color.Red,
                        Fields = new List<EmbedFieldBuilder> {
                            new EmbedFieldBuilder() {
                                Name = "Enable",
                                Value = "Use `/addons install Ticketing` to enable this addon."
                            }
                        }
                    }.Build());
                    return;
                }
                var ticket = await ticketsRepository.GetTicketsModelAsync(Context.Guild.Id, Context.Channel.Id);
                if (ticket.IntroMessageId == null
                    || await Context.Channel.GetMessageAsync((ulong)ticket.IntroMessageId) == null) {
                    await RespondAsync(TicketsLocale.TicketsError_notTicket, ephemeral: true);
                    return;
                }
                if (!ticket.UserIds.Contains((long)Context.User.Id) || !ticket.ClaimedUserId.Contains((long)Context.User.Id)) {
                    await RespondAsync(TicketsLocale.TicketsError_notOwner, ephemeral: true);
                    return;
                }
                await RespondAsync(TicketsLocale.TicketsClosed);
                await ((SocketGuildChannel)Context.Channel).DeleteAsync();
                await ticketsRepository.RemoveTicketAsync(Context.Guild.Id, Context.Channel.Id);
                await ticketsRepository.SaveAsync();
            }

            [SlashCommand("claim", "Claims a ticket", runMode: RunMode.Async)]
            public async Task ClaimTicket() {
                if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Ticketing")) {
                    await RespondAsync(embed: new EmbedBuilder {
                        Title = "Ticketing",
                        Description = "This addon is disabled on this server.",
                        Color = Color.Red,
                        Fields = new List<EmbedFieldBuilder> {
                            new EmbedFieldBuilder() {
                                Name = "Enable",
                                Value = "Use `/addons install Ticketing` to enable this addon."
                            }
                        }
                    }.Build());
                    return;
                }
                if (!((SocketGuildUser)Context.User).GuildPermissions.Administrator) {
                    await RespondAsync(TicketsLocale.TicketsError_noPermission, ephemeral: true);
                    return;
                }
                var ticket = await ticketsRepository.GetTicketsModelAsync(Context.Guild.Id, Context.Channel.Id);
                if (ticket.IntroMessageId == null
                    || await Context.Channel.GetMessageAsync((ulong)ticket.IntroMessageId) == null) {
                    await RespondAsync(TicketsLocale.TicketsError_notTicket, ephemeral: true);
                    return;
                }
                if (ticket.ClaimedUserId.Contains((long)Context.User.Id)) {
                    await RespondAsync(TicketsLocale.TicketsError_alreadyClaimed, ephemeral: true);
                    return;
                }
                await ((SocketGuildChannel)Context.Channel).AddPermissionOverwriteAsync(Context.User, new OverwritePermissions(
                    addReactions: PermValue.Allow,
                    attachFiles: PermValue.Allow,
                    embedLinks: PermValue.Allow,
                    readMessageHistory: PermValue.Allow,
                    sendMessages: PermValue.Allow,
                    viewChannel: PermValue.Allow,
                    useApplicationCommands: PermValue.Allow
                ));
                await ticketsRepository.AddTicketClaimedUserIdAsync(Context.Guild.Id, Context.Channel.Id, Context.User.Id);
                await ticketsRepository.SaveAsync();
                await Context.Channel.SendMessageAsync(embed: new EmbedBuilder {
                    Title = TicketsLocale.TicketsEmbedClaim_title,
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = TicketsLocale.TicketsEmbedClaim_fieldName,
                            Value = Context.User.Username
                        }
                    },
                    Color = Color.Green
                }.Build());
                await RespondAsync(TicketsLocale.TicketsClaimed, ephemeral: true);

            }

            [SlashCommand("unclaim", "Unclaims a ticket", runMode: RunMode.Async)]
            public async Task UnclaimTicket() {
                if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Ticketing")) {
                    await RespondAsync(embed: new EmbedBuilder {
                        Title = "Ticketing",
                        Description = "This addon is disabled on this server.",
                        Color = Color.Red,
                        Fields = new List<EmbedFieldBuilder> {
                            new EmbedFieldBuilder() {
                                Name = "Enable",
                                Value = "Use `/addons install Ticketing` to enable this addon."
                            }
                        }
                    }.Build());
                    return;
                }
                var ticket = await ticketsRepository.GetTicketsModelAsync(Context.Guild.Id, Context.Channel.Id);
                if (ticket.IntroMessageId == null ||
                    await Context.Channel.GetMessageAsync((ulong)ticket.IntroMessageId) == null) {
                    await RespondAsync(TicketsLocale.TicketsError_notTicket, ephemeral: true);
                    return;
                }
                if (!ticket.ClaimedUserId.Contains((long)Context.User.Id)) {
                    await RespondAsync(TicketsLocale.TicketsError_notClaimed, ephemeral: true);
                    return;
                }
                await ((SocketGuildChannel)Context.Channel).RemovePermissionOverwriteAsync(Context.User);
                await ticketsRepository.RemoveTicketClaimedUserIdAsync(Context.Guild.Id, Context.Channel.Id, Context.User.Id);
                await ticketsRepository.SaveAsync();
                await Context.Channel.SendMessageAsync(embed: new EmbedBuilder {
                    Title = TicketsLocale.TicketsEmbedUnclaim_title,
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = TicketsLocale.TicketsEmbedUnclaim_fieldName,
                            Value = Context.User.Username
                        }
                    },
                    Color = Color.Green
                }.Build());
                await RespondAsync(TicketsLocale.TicketsUnclaimed, ephemeral: true);
            }

            [SlashCommand("adduser", "Adds a user to a ticket", runMode: RunMode.Async)]
            public async Task AddUserToTicket(IUser user) {
                if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Ticketing")) {
                    await RespondAsync(embed: new EmbedBuilder {
                        Title = "Ticketing",
                        Description = "This addon is disabled on this server.",
                        Color = Color.Red,
                        Fields = new List<EmbedFieldBuilder> {
                            new EmbedFieldBuilder() {
                                Name = "Enable",
                                Value = "Use `/addons install Ticketing` to enable this addon."
                            }
                        }
                    }.Build());
                    return;
                }
                var ticket = await ticketsRepository.GetTicketsModelAsync(Context.Guild.Id, Context.Channel.Id);
                if (ticket.IntroMessageId == null || await Context.Channel.GetMessageAsync((ulong)ticket.IntroMessageId) == null) {
                    await RespondAsync(TicketsLocale.TicketsError_notTicket, ephemeral: true);
                    return;
                }
                if (!(ticket.UserIds.Contains((long)Context.User.Id) || ticket.ClaimedUserId.Contains((long)Context.User.Id))) {
                    await RespondAsync(TicketsLocale.TicketsError_notMember, ephemeral: true);
                    return;
                }
                if (ticket.UserIds.Contains((long)user.Id) || ticket.ClaimedUserId.Contains((long)user.Id)) {
                    await RespondAsync(TicketsLocale.TicketsError_alreadyMember_user, ephemeral: true);
                    return;
                }
                await ticketsRepository.AddTicketUserIdAsync(Context.Guild.Id, Context.Channel.Id, user.Id);
                await ticketsRepository.SaveAsync();
                await ((SocketGuildChannel)Context.Channel).AddPermissionOverwriteAsync(user, new OverwritePermissions(
                    addReactions: PermValue.Allow,
                    attachFiles: PermValue.Allow,
                    embedLinks: PermValue.Allow,
                    readMessageHistory: PermValue.Allow,
                    sendMessages: PermValue.Allow,
                    viewChannel: PermValue.Allow,
                    useApplicationCommands: PermValue.Allow
                ));
                await Context.Channel.SendMessageAsync(embed: new EmbedBuilder {
                    Title = TicketsLocale.TicketsEmbedAddUser_title,
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = TicketsLocale.TicketsEmbedAddUser_fieldName,
                            Value = user.Username
                        }
                    },
                    Color = Color.Green
                }.Build());
                await RespondAsync(TicketsLocale.TicketsUserAdded, ephemeral: true);
            }

            [SlashCommand("removeuser", "Removes a user from a ticket", runMode: RunMode.Async)]
            public async Task RemoveUserFromTicket(IUser user) {
                if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Ticketing")) {
                    await RespondAsync(embed: new EmbedBuilder {
                        Title = "Ticketing",
                        Description = "This addon is disabled on this server.",
                        Color = Color.Red,
                        Fields = new List<EmbedFieldBuilder> {
                            new EmbedFieldBuilder() {
                                Name = "Enable",
                                Value = "Use `/addons install Ticketing` to enable this addon."
                            }
                        }
                    }.Build());
                    return;
                }
                var ticket = await ticketsRepository.GetTicketsModelAsync(Context.Guild.Id, Context.Channel.Id);
                if (ticket.IntroMessageId == null || await Context.Channel.GetMessageAsync((ulong)ticket.IntroMessageId) == null) {
                    await RespondAsync(TicketsLocale.TicketsError_notTicket, ephemeral: true);
                    return;
                }
                if (!(ticket.UserIds.Contains((long)Context.User.Id) || ticket.ClaimedUserId.Contains((long)Context.User.Id))) {
                    await RespondAsync(TicketsLocale.TicketsError_notMember, ephemeral: true);
                    return;
                }
                if (!(ticket.UserIds.Contains((long)user.Id) || ticket.ClaimedUserId.Contains((long)user.Id))) {
                    await RespondAsync(TicketsLocale.TicketsError_notMember_user, ephemeral: true);
                    return;
                }
                await ticketsRepository.RemoveTicketClaimedUserIdAsync(Context.Guild.Id, Context.Channel.Id, user.Id);
                await ticketsRepository.RemoveTicketUserIdAsync(Context.Guild.Id, Context.Channel.Id, user.Id);
                await ticketsRepository.SaveAsync();
                await ((SocketGuildChannel)Context.Channel).RemovePermissionOverwriteAsync(user);
                await Context.Channel.SendMessageAsync(embed: new EmbedBuilder {
                    Title = TicketsLocale.TicketsEmbedRemoveUser_title,
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = TicketsLocale.TicketsEmbedRemoveUser_fieldName,
                            Value = user.Username
                        }
                    },
                    Color = Color.Green
                }.Build());
                await RespondAsync(TicketsLocale.TicketsUserRemoved, ephemeral: true);
            }

            [SlashCommand("leave", "Leaves a ticket", runMode: RunMode.Async)]
            public async Task LeaveTicket() {
                if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Ticketing")) {
                    await RespondAsync(embed: new EmbedBuilder {
                        Title = "Ticketing",
                        Description = "This addon is disabled on this server.",
                        Color = Color.Red,
                        Fields = new List<EmbedFieldBuilder> {
                            new EmbedFieldBuilder() {
                                Name = "Enable",
                                Value = "Use `/addons install Ticketing` to enable this addon."
                            }
                        }
                    }.Build());
                    return;
                }
                var ticket = await ticketsRepository.GetTicketsModelAsync(Context.Guild.Id, Context.Channel.Id);
                if (ticket.IntroMessageId == null || await Context.Channel.GetMessageAsync((ulong)ticket.IntroMessageId) == null) {
                    await RespondAsync(TicketsLocale.TicketsError_notTicket, ephemeral: true);
                    return;
                }
                if (!(ticket.UserIds.Contains((long)Context.User.Id) || ticket.ClaimedUserId.Contains((long)Context.User.Id))) {
                    await RespondAsync(TicketsLocale.TicketsError_notMember, ephemeral: true);
                    return;
                }
                await ticketsRepository.RemoveTicketClaimedUserIdAsync(Context.Guild.Id, Context.Channel.Id, Context.User.Id);
                await ticketsRepository.RemoveTicketUserIdAsync(Context.Guild.Id, Context.Channel.Id, Context.User.Id);
                await ticketsRepository.SaveAsync();
                await Context.Channel.SendMessageAsync(embed: new EmbedBuilder {
                    Title = TicketsLocale.TicketsEmbedRemoveUser_title,
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = TicketsLocale.TicketsEmbedRemoveUser_fieldName,
                            Value = Context.User.Username
                        }
                    },
                    Color = Color.Green
                }.Build());
                await RespondAsync(TicketsLocale.TicketsUserRemoved, ephemeral: true);
            }

            public async Task OnButtonExecutedEvent(SocketMessageComponent messageComponent) {
                if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Ticketing")) {
                    return;
                }
                SocketUserMessage message = messageComponent.Message;
                SocketGuildChannel channel = (SocketGuildChannel)message.Channel;
                SocketGuild guild = channel.Guild;
                SocketGuildUser user = (SocketGuildUser)messageComponent.User;
                TicketsModel ticket = await ticketsRepository.GetTicketsModelAsync(guild.Id, channel.Id);
                if (message.Id == _closeConfirmId) {
                    switch(messageComponent.Data.CustomId) {
                        case "close-yes":
                            await messageComponent.RespondAsync(TicketsLocale.TicketsClosed);
                            await ((SocketGuildChannel)message.Channel).DeleteAsync();
                            await ticketsRepository.RemoveTicketAsync(channel.Guild.Id, channel.Id);
                            await ticketsRepository.SaveAsync();
                            break;
                        case "close-no":
                            await messageComponent.RespondAsync(TicketsLocale.TicketsClosedCancel);
                            break;
                    }
                } else if ((long)message.Id == ticket.IntroMessageId) {
                    switch(messageComponent.Data.CustomId) {
                        case "close":
                            await messageComponent.RespondAsync(embed: new EmbedBuilder {
                                Title = TicketsLocale.TicketsEmbedCloseConfirm_title,
                                Fields = new List<EmbedFieldBuilder> {
                                    new EmbedFieldBuilder {
                                        Name = TicketsLocale.TicketsEmbedCloseConfirm_fieldName,
                                        Value = TicketsLocale.TicketsEmbedCloseConfirm_fieldValue
                                    }
                                },
                                Color = Color.Red
                            }.Build(), components: new ComponentBuilder()
                                .WithButton(PollLocale.PollDefaultOption1, "close-yes")
                                .WithButton(PollLocale.PollDefaultOption2, "close-no")
                                .Build());
                            _closeConfirmId = (await messageComponent.GetOriginalResponseAsync()).Id;
                            return;
                        case "claim" when !user.GuildPermissions.Administrator:
                            await messageComponent.RespondAsync(TicketsLocale.TicketsError_noPermission, ephemeral: true);
                            return;
                        case "claim":
                            var claimedUsers = await ticketsRepository.GetTicketsModelAsync(guild.Id, channel.Id);
                            if (claimedUsers.ClaimedUserId.Contains((long)user.Id)) {
                                await messageComponent.RespondAsync(TicketsLocale.TicketsError_alreadyClaimed, ephemeral: true);
                                return;
                            }
                            await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(
                                addReactions: PermValue.Allow,
                                attachFiles: PermValue.Allow,
                                embedLinks: PermValue.Allow,
                                readMessageHistory: PermValue.Allow,
                                sendMessages: PermValue.Allow,
                                viewChannel: PermValue.Allow,
                                useApplicationCommands: PermValue.Allow
                            ));
                            await ticketsRepository.AddTicketClaimedUserIdAsync(channel.Guild.Id, channel.Id, user.Id);
                            await ticketsRepository.SaveAsync();
                            await message.Channel.SendMessageAsync(embed: new EmbedBuilder {
                                Title = TicketsLocale.TicketsEmbedClaim_title,
                                Fields = new List<EmbedFieldBuilder> {
                                    new EmbedFieldBuilder {
                                        Name = TicketsLocale.TicketsEmbedClaim_fieldName,
                                        Value = user.Username
                                    }
                                },
                                Color = Color.Green
                            }.Build());
                            await messageComponent.RespondAsync(TicketsLocale.TicketsClaimed, ephemeral: true);
                            break;
                        default:
                            await messageComponent.RespondAsync(TicketsLocale.TicketsError_notTicket, ephemeral: true);
                            return;
                    }
                }
            }
        }
    }
}
