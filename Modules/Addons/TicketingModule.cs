using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Database.Models.Bot;
using Finder.Database.Repositories.Bot;

namespace Finder.Bot.Modules.Addons {
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
                        Fields = new List<EmbedFieldBuilder> {
                            new EmbedFieldBuilder {
                                Name = "Enable",
                                Value = "Use `/addons install Ticketing` to enable this addon."
                            }
                        }
                    }.Build());
                    return;
                }
                if (name == null) {
                    await RespondAsync("Please specify a name for the ticket.");
                    return;
                }
                if (name.Length > 32) {
                    await RespondAsync("The name of the ticket is too long.");
                    return;
                }
                var supportChannel = await Context.Guild.CreateTextChannelAsync($"ticket-{name}", x => {
                    x.PermissionOverwrites = new List<Overwrite> {
                        new Overwrite(Context.Guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(readMessageHistory: PermValue.Deny, sendMessages: PermValue.Deny, viewChannel: PermValue.Deny)),
                        new Overwrite(Context.User.Id, PermissionTarget.User, new OverwritePermissions(addReactions: PermValue.Allow, attachFiles: PermValue.Allow, embedLinks: PermValue.Allow, readMessageHistory: PermValue.Allow, sendMessages: PermValue.Allow, viewChannel: PermValue.Allow, useApplicationCommands: PermValue.Allow))
                    };
                });
                var message = await supportChannel.SendMessageAsync(embed: new EmbedBuilder {
                    Title = "Ticket",
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = name,
                            Value = $"Channel made by {Context.User.Username}"
                        }
                    }
                }.Build(), components: new ComponentBuilder { 
                    ActionRows = new List<ActionRowBuilder> {
                        new ActionRowBuilder {
                            Components = new List<IMessageComponent> {
                                new ButtonBuilder {
                                    CustomId = "close",
                                    Label = "Close Ticket"
                                }.Build(),
                                new ButtonBuilder {
                                    CustomId = "claim",
                                    Label = "Claim Ticket"
                                }.Build()
                            }
                        }
                    }
                }.Build());
                await ticketsRepository.AddTicketAsync(Context.Guild.Id, supportChannel.Id, message.Id, new List<long?> { (long)Context.User.Id }, name, new List<long>());
                await ticketsRepository.SaveAsync();
                await RespondAsync(embed: new EmbedBuilder {
                    Title = "Ticket Created",
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = string.Format("Opened a new ticket:"),
                            Value = supportChannel.Mention
                        }
                    }
                }.Build());
            }

            [SlashCommand("close", "Closes a ticket", runMode: RunMode.Async)]
            public async Task CloseTicket() {
                if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Ticketing")) {
                    await RespondAsync(embed: new EmbedBuilder {
                        Title = "Ticketing",
                        Description = "This addon is disabled on this server.",
                        Fields = new List<EmbedFieldBuilder> {
                            new EmbedFieldBuilder {
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
                    await RespondAsync("You are not in a ticket channel.", ephemeral: true);
                    return;
                }
                if (!ticket.UserIds.Contains((long)Context.User.Id) || !ticket.ClaimedUserId.Contains((long)Context.User.Id)) {
                    await RespondAsync("You are not the owner of this ticket.", ephemeral: true);
                    return;
                }
                await RespondAsync("Ticket Closed");
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
                        Fields = new List<EmbedFieldBuilder> {
                            new EmbedFieldBuilder {
                                Name = "Enable",
                                Value = "Use `/addons install Ticketing` to enable this addon."
                            }
                        }
                    }.Build());
                    return;
                }
                if (!((SocketGuildUser)Context.User).GuildPermissions.Administrator) {
                    await RespondAsync("You do not have permission to claim a ticket.", ephemeral: true);
                    return;
                }
                var ticket = await ticketsRepository.GetTicketsModelAsync(Context.Guild.Id, Context.Channel.Id);
                if (ticket.IntroMessageId == null
                    || await Context.Channel.GetMessageAsync((ulong)ticket.IntroMessageId) == null) {
                    await RespondAsync("You are not in a ticket channel.", ephemeral: true);
                    return;
                }
                if (ticket.ClaimedUserId.Contains((long)Context.User.Id)) {
                    await RespondAsync("You have already claimed this ticket.", ephemeral: true);
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
                    Title = "Ticket Claimed",
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = "Claimed By",
                            Value = Context.User.Username
                        }
                    }
                }.Build());
                await RespondAsync("You have claimed this ticket.", ephemeral: true);

            }

            [SlashCommand("unclaim", "Unclaims a ticket", runMode: RunMode.Async)]
            public async Task UnclaimTicket() {
                if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Ticketing")) {
                    await RespondAsync(embed: new EmbedBuilder {
                        Title = "Ticketing",
                        Description = "This addon is disabled on this server.",
                        Fields = new List<EmbedFieldBuilder> {
                            new EmbedFieldBuilder {
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
                    await RespondAsync("You are not in a ticket channel.", ephemeral: true);
                    return;
                }
                if (!ticket.ClaimedUserId.Contains((long)Context.User.Id)) {
                    await RespondAsync("You have not claimed this ticket.", ephemeral: true);
                    return;
                }
                await ((SocketGuildChannel)Context.Channel).RemovePermissionOverwriteAsync(Context.User);
                await ticketsRepository.RemoveTicketClaimedUserIdAsync(Context.Guild.Id, Context.Channel.Id, Context.User.Id);
                await ticketsRepository.SaveAsync();
                await Context.Channel.SendMessageAsync(embed: new EmbedBuilder {
                    Title = "Ticket Unclaimed",
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = "User",
                            Value = Context.User.Username
                        }
                    }
                }.Build());
                await RespondAsync("You have unclaimed this ticket.", ephemeral: true);
            }

            [SlashCommand("adduser", "Adds a user to a ticket", runMode: RunMode.Async)]
            public async Task AddUserToTicket(IUser user) {
                if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Ticketing")) {
                    await RespondAsync(embed: new EmbedBuilder {
                        Title = "Ticketing",
                        Description = "This addon is disabled on this server.",
                        Fields = new List<EmbedFieldBuilder> {
                            new EmbedFieldBuilder {
                                Name = "Enable",
                                Value = "Use `/addons install Ticketing` to enable this addon."
                            }
                        }
                    }.Build());
                    return;
                }
                var ticket = await ticketsRepository.GetTicketsModelAsync(Context.Guild.Id, Context.Channel.Id);
                if (ticket.IntroMessageId == null || await Context.Channel.GetMessageAsync((ulong)ticket.IntroMessageId) == null) {
                    await RespondAsync("You are not in a ticket channel.", ephemeral: true);
                    return;
                }
                if (!(ticket.UserIds.Contains((long)Context.User.Id) || ticket.ClaimedUserId.Contains((long)Context.User.Id))) {
                    await RespondAsync("You are not a member of this ticket.", ephemeral: true);
                    return;
                }
                if (ticket.UserIds.Contains((long)user.Id) || ticket.ClaimedUserId.Contains((long)user.Id)) {
                    await RespondAsync("This user is already a member of this ticket.", ephemeral: true);
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
                    Title = "User Added",
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = "User",
                            Value = user.Username
                        }
                    }
                }.Build());
                await RespondAsync("User added.", ephemeral: true);
            }

            [SlashCommand("removeuser", "Removes a user from a ticket", runMode: RunMode.Async)]
            public async Task RemoveUserFromTicket(IUser user) {
                if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Ticketing")) {
                    await RespondAsync(embed: new EmbedBuilder {
                        Title = "Ticketing",
                        Description = "This addon is disabled on this server.",
                        Fields = new List<EmbedFieldBuilder> {
                            new EmbedFieldBuilder {
                                Name = "Enable",
                                Value = "Use `/addons install Ticketing` to enable this addon."
                            }
                        }
                    }.Build());
                    return;
                }
                var ticket = await ticketsRepository.GetTicketsModelAsync(Context.Guild.Id, Context.Channel.Id);
                if (ticket.IntroMessageId == null || await Context.Channel.GetMessageAsync((ulong)ticket.IntroMessageId) == null) {
                    await RespondAsync("You are not in a ticket channel.", ephemeral: true);
                    return;
                }
                if (!(ticket.UserIds.Contains((long)Context.User.Id) || ticket.ClaimedUserId.Contains((long)Context.User.Id))) {
                    await RespondAsync("You are not a member of this ticket.", ephemeral: true);
                    return;
                }
                if (!(ticket.UserIds.Contains((long)user.Id) || ticket.ClaimedUserId.Contains((long)user.Id))) {
                    await RespondAsync("This user is not a member of this ticket.", ephemeral: true);
                    return;
                }
                await ticketsRepository.RemoveTicketClaimedUserIdAsync(Context.Guild.Id, Context.Channel.Id, user.Id);
                await ticketsRepository.RemoveTicketUserIdAsync(Context.Guild.Id, Context.Channel.Id, user.Id);
                await ticketsRepository.SaveAsync();
                await ((SocketGuildChannel)Context.Channel).RemovePermissionOverwriteAsync(user);
                await Context.Channel.SendMessageAsync(embed: new EmbedBuilder {
                    Title = "User Removed",
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = "User",
                            Value = user.Username
                        }
                    }
                }.Build());
                await RespondAsync("User removed.", ephemeral: true);
            }

            [SlashCommand("leave", "Leaves a ticket", runMode: RunMode.Async)]
            public async Task LeaveTicket() {
                if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Ticketing")) {
                    await RespondAsync(embed: new EmbedBuilder {
                        Title = "Ticketing",
                        Description = "This addon is disabled on this server.",
                        Color = Color.Red,
                        Fields = new List<EmbedFieldBuilder> {
                            new EmbedFieldBuilder {
                                Name = "Enable",
                                Value = "Use `/addons install Ticketing` to enable this addon."
                            }
                        }
                    }.Build());
                    return;
                }
                var ticket = await ticketsRepository.GetTicketsModelAsync(Context.Guild.Id, Context.Channel.Id);
                if (ticket.IntroMessageId == null || await Context.Channel.GetMessageAsync((ulong)ticket.IntroMessageId) == null) {
                    await RespondAsync("You are not in a ticket channel.", ephemeral: true);
                    return;
                }
                if (!(ticket.UserIds.Contains((long)Context.User.Id) || ticket.ClaimedUserId.Contains((long)Context.User.Id))) {
                    await RespondAsync("You are not a member of this ticket.", ephemeral: true);
                    return;
                }
                await ticketsRepository.RemoveTicketClaimedUserIdAsync(Context.Guild.Id, Context.Channel.Id, Context.User.Id);
                await ticketsRepository.RemoveTicketUserIdAsync(Context.Guild.Id, Context.Channel.Id, Context.User.Id);
                await ticketsRepository.SaveAsync();
                await Context.Channel.SendMessageAsync(embed: new EmbedBuilder {
                    Title = "User Removed",
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = "User",
                            Value = Context.User.Username
                        }
                    },
                    Color = Color.Green
                }.Build());
                await RespondAsync("User removed.", ephemeral: true);
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
                            await messageComponent.RespondAsync("Ticket Closed");
                            await ((SocketGuildChannel)message.Channel).DeleteAsync();
                            await ticketsRepository.RemoveTicketAsync(channel.Guild.Id, channel.Id);
                            await ticketsRepository.SaveAsync();
                            break;
                        case "close-no":
                            await messageComponent.RespondAsync("You have cancelled closing this ticket.");
                            break;
                    }
                } else if ((long)message.Id == ticket.IntroMessageId) {
                    switch(messageComponent.Data.CustomId) {
                        case "close":
                            await messageComponent.RespondAsync(embed: new EmbedBuilder {
                                Title = "Are you sure?",
                                Fields = new List<EmbedFieldBuilder> {
                                    new EmbedFieldBuilder {
                                        Name = "Close Ticket",
                                        Value = "This will close the ticket and delete the channel."
                                    }
                                },
                                Color = Color.Red
                            }.Build(), components: new ComponentBuilder()
                                .WithButton("Yes", "close-yes")
                                .WithButton("No", "close-no")
                                .Build());
                            _closeConfirmId = (await messageComponent.GetOriginalResponseAsync()).Id;
                            return;
                        case "claim" when !user.GuildPermissions.Administrator:
                            await messageComponent.RespondAsync("You do not have permission to claim a ticket.", ephemeral: true);
                            return;
                        case "claim":
                            var claimedUsers = await ticketsRepository.GetTicketsModelAsync(guild.Id, channel.Id);
                            if (claimedUsers.ClaimedUserId.Contains((long)user.Id)) {
                                await messageComponent.RespondAsync("You have already claimed this ticket.", ephemeral: true);
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
                                Title = "Ticket Claimed",
                                Fields = new List<EmbedFieldBuilder> {
                                    new EmbedFieldBuilder {
                                        Name = "Claimed By",
                                        Value = user.Username
                                    }
                                },
                                Color = Color.Green
                            }.Build());
                            await messageComponent.RespondAsync("You have claimed this ticket.", ephemeral: true);
                            break;
                        default:
                            await messageComponent.RespondAsync("You are not in a ticket channel.", ephemeral: true);
                            return;
                    }
                }
            }
        }
    }
}
