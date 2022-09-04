using Discord;
using Discord.Interactions;
using Finder.Bot.Repositories.Bot;

namespace Finder.Bot.Modules.Addons {
    public class EconomyModule : InteractionModuleBase<ShardedInteractionContext> {
        private readonly EconomyRepository economyRepository;
        private readonly AddonsRepository addonsRepository;
        public EconomyModule(EconomyRepository _economyRepository, AddonsRepository _addonsRepository) {
            economyRepository = _economyRepository;
            addonsRepository = _addonsRepository;
        }
        
        [SlashCommand("balance", "Checks user's balance.", runMode: RunMode.Async)]
        public async Task Balance(IUser? user = null) {
            if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Economy")) {
                await RespondAsync(embed: new EmbedBuilder {
                    Title = "Economy",
                    Description = "This addon is disabled on this server.",
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = "Enable",
                            Value = "Use `/addons install Economy` to enable this addon."
                        }
                    }
                }.Build());
                return;
            }
            user ??= Context.User;
            var economy = await economyRepository.GetEconomyModelAsync(Context.Guild.Id, user.Id);
            await RespondAsync(embed: new EmbedBuilder {
                Title = $"{user.Username}\'s balance",
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder {
                        Name = "Money",
                        Value = economy.Money.ToString()
                    },
                    new EmbedFieldBuilder {
                        Name = "Bank",
                        Value = economy.Bank.ToString()
                    }
                }
            }.Build());
        }

        [SlashCommand("deposit", "Deposits money into your bank.", runMode: RunMode.Async)]
        public async Task Deposit(int amount) {
            if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Economy")) {
                await RespondAsync(embed: new EmbedBuilder {
                    Title = "Economy",
                    Description = "This addon is disabled on this server.",
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = "Enable",
                            Value = "Use `/addons install Economy` to enable this addon."
                        }
                    }
                }.Build());
                return;
            }
            var economy = await economyRepository.GetEconomyModelAsync(Context.Guild.Id, Context.User.Id);
            if (economy.Money < amount) {
                await RespondAsync("You don\'t have enough money.");
                return;
            }
            await economyRepository.AddEconomyAsync(Context.Guild.Id, Context.User.Id, -amount, amount);
            await economyRepository.SaveAsync();
            await RespondAsync(embed: new EmbedBuilder {
                Title = "Deposit",
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder {
                        Name = "You deposited",
                        Value = amount.ToString()
                    }
                }
            }.Build());
        }

        [SlashCommand("withdraw", "Withdraws money from your bank.", runMode: RunMode.Async)]
        public async Task Withdraw(int amount) {
            if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Economy")) {
                await RespondAsync(embed: new EmbedBuilder {
                    Title = "Economy",
                    Description = "This addon is disabled on this server.",
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = "Enable",
                            Value = "Use `/addons install Economy` to enable this addon."
                        }
                    }
                }.Build());
                return;
            }
            var economy = await economyRepository.GetEconomyModelAsync(Context.Guild.Id, Context.User.Id);
            if (economy.Bank < amount) {
                await RespondAsync("You don\'t have enough money in your bank.");
                return;
            }
            await economyRepository.AddEconomyAsync(Context.Guild.Id, Context.User.Id, amount, -amount);
            await economyRepository.SaveAsync();
            await RespondAsync(embed: new EmbedBuilder {
                Title = "Withdraw",
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder {
                        Name = "You withdrew",
                        Value = amount.ToString()
                    }
                }
            }.Build());
        }

        [SlashCommand("pay", "Pays money to another user.", runMode: RunMode.Async)]
        public async Task Pay(IUser user, int amount) {
            if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Economy")) {
                await RespondAsync(embed: new EmbedBuilder {
                    Title = "Economy",
                    Description = "This addon is disabled on this server.",
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = "Enable",
                            Value = "Use `/addons install Economy` to enable this addon."
                        }
                    }
                }.Build());
                return;
            }
            var economy = await economyRepository.GetEconomyModelAsync(Context.Guild.Id, Context.User.Id);
            if (economy.Money < amount) {
                await RespondAsync("You don\'t have enough money.");
                return;
            }
            await economyRepository.AddEconomyAsync(Context.Guild.Id, Context.User.Id, -amount, 0);
            await economyRepository.AddEconomyAsync(Context.Guild.Id, user.Id, amount, 0);
            await economyRepository.SaveAsync();
            await RespondAsync(embed: new EmbedBuilder {
                Title = "Pay",
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder {
                        Name = "Payee",
                        Value = user.Username
                    },
                    new EmbedFieldBuilder {
                        Name = "Amount",
                        Value = amount.ToString()
                    }
                }
            }.Build());
        }

        [SlashCommand("transfer", "Transfers money to another user from your bank.", runMode: RunMode.Async)]
        public async Task Transfer(IUser user, int amount) {
            if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Economy")) {
                await RespondAsync(embed: new EmbedBuilder {
                    Title = "Economy",
                    Description = "This addon is disabled on this server.",
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = "Enable",
                            Value = "Use `/addons install Economy` to enable this addon."
                        }
                    }
                }.Build());
                return;
            }
            var economy = await economyRepository.GetEconomyModelAsync(Context.Guild.Id, Context.User.Id);
            if (economy.Bank < amount) {
                await RespondAsync("You don\'t have enough money in your bank.");
                return;
            }
            await economyRepository.AddEconomyAsync(Context.Guild.Id, Context.User.Id, 0, -amount);
            await economyRepository.AddEconomyAsync(Context.Guild.Id, user.Id, 0, amount);
            await economyRepository.SaveAsync();
            await RespondAsync(embed: new EmbedBuilder {
                Title = "Transfer",
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder {
                        Name = "Payee",
                        Value = user.Username
                    },
                    new EmbedFieldBuilder {
                        Name = "Amount",
                        Value = amount.ToString()
                    }
                }
            }.Build());
        }

        [SlashCommand("setbalance", "Sets the balance of a user.", runMode: RunMode.Async)]
        public async Task SetBalance(IUser user, int amount) {
            if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Economy")) {
                await RespondAsync(embed: new EmbedBuilder {
                    Title = "Economy",
                    Description = "This addon is disabled on this server.",
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = "Enable",
                            Value = "Use `/addons install Economy` to enable this addon."
                        }
                    }
                }.Build());
                return;
            }
            await economyRepository.AddEconomyAsync(Context.Guild.Id, user.Id, amount, 0);
            await economyRepository.SaveAsync();
            await RespondAsync(embed: new EmbedBuilder {
                Title = "Set Balance",
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder {
                        Name = "User",
                        Value = user.Username
                    },
                    new EmbedFieldBuilder {
                        Name = "Amount",
                        Value = amount.ToString()
                    }
                }
            }.Build());
        }
    }
}
