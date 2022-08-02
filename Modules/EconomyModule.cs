using Discord;
using Discord.Interactions;
using Finder.Database.Repositories;
using Finder.Bot.Resources;
using Finder.Database.Repositories.Bot;

namespace Finder.Bot.Modules {
    public class EconomyModule : InteractionModuleBase<ShardedInteractionContext> {
        // todo: permissions
        private readonly EconomyRepository economyRepository;
        public EconomyModule(EconomyRepository _economyRepository) {
            economyRepository = _economyRepository;
        }
        [SlashCommand("balance", "Checks user's balance.", runMode: RunMode.Async)]
        public async Task Balance(IUser? user = null) {
            user ??= Context.User;
            var economy = await economyRepository.GetEconomyAsync(Context.Guild.Id, user.Id);
            await RespondAsync(embed: new EmbedBuilder() {
                Title = string.Format(EconomyLocale.EconomyEmbedBal_title, user.Username),
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder() {
                        Name = EconomyLocale.EconomyEmbedBal_field0Name,
                        Value = economy.Money.ToString()
                    },
                    new EmbedFieldBuilder() {
                        Name = EconomyLocale.EconomyEmbedBal_field1Name,
                        Value = economy.Bank.ToString()
                    }
                },
                Color = Color.Green
            }.Build());
        }

        [SlashCommand("deposit", "Deposits money into your bank.", runMode: RunMode.Async)]
        public async Task Deposit(int amount) {
            var economy = await economyRepository.GetEconomyAsync(Context.Guild.Id, Context.User.Id);
            if (economy.Money < amount) {
                await RespondAsync(EconomyLocale.EconomyError_notEnoughMoney);
                return;
            }
            await economyRepository.AddEconomyAsync(Context.Guild.Id, Context.User.Id, -amount, amount);
            await economyRepository.SaveAsync();
            await RespondAsync(embed: new EmbedBuilder() {
                Title = EconomyLocale.EconomyEmbedDeposit_title,
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder() {
                        Name = EconomyLocale.EconomyEmbedDeposit_fieldName,
                        Value = amount.ToString()
                    }
                },
                Color = Color.Green
            }.Build());
        }

        [SlashCommand("withdraw", "Withdraws money from your bank.", runMode: RunMode.Async)]
        public async Task Withdraw(int amount) {
            var economy = await economyRepository.GetEconomyAsync(Context.Guild.Id, Context.User.Id);
            if (economy.Bank < amount) {
                await RespondAsync(EconomyLocale.EconomyError_notEnoughMoneyBank);
                return;
            }
            await economyRepository.AddEconomyAsync(Context.Guild.Id, Context.User.Id, amount, -amount);
            await economyRepository.SaveAsync();
            await RespondAsync(embed: new EmbedBuilder() {
                Title = EconomyLocale.EconomyEmbedWithdraw_title,
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder() {
                        Name = EconomyLocale.EconomyEmbedWithdraw_fieldName,
                        Value = amount.ToString()
                    }
                },
                Color = Color.Green
            }.Build());
        }

        [SlashCommand("pay", "Pays money to another user.", runMode: RunMode.Async)]
        public async Task Pay(IUser user, int amount) {
            var economy = await economyRepository.GetEconomyAsync(Context.Guild.Id, Context.User.Id);
            if (economy.Money < amount) {
                await RespondAsync(EconomyLocale.EconomyError_notEnoughMoney);
                return;
            }
            await economyRepository.AddEconomyAsync(Context.Guild.Id, Context.User.Id, -amount, 0);
            await economyRepository.AddEconomyAsync(Context.Guild.Id, user.Id, amount, 0);
            await economyRepository.SaveAsync();
            await RespondAsync(embed: new EmbedBuilder() {
                Title = EconomyLocale.EconomyEmbedPay_title,
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder() {
                        Name = EconomyLocale.EconomyEmbedPay_field0Name,
                        Value = user.Username
                    },
                    new EmbedFieldBuilder() {
                        Name = EconomyLocale.EconomyEmbedPay_field1Name,
                        Value = amount.ToString()
                    }
                },
                Color = Color.Green
            }.Build());
        }

        [SlashCommand("transfer", "Transfers money to another user from your bank.", runMode: RunMode.Async)]
        public async Task Transfer(IUser user, int amount) {
            var economy = await economyRepository.GetEconomyAsync(Context.Guild.Id, Context.User.Id);
            if (economy.Bank < amount) {
                await RespondAsync(EconomyLocale.EconomyError_notEnoughMoneyBank);
                return;
            }
            await economyRepository.AddEconomyAsync(Context.Guild.Id, Context.User.Id, 0, -amount);
            await economyRepository.AddEconomyAsync(Context.Guild.Id, user.Id, 0, amount);
            await economyRepository.SaveAsync();
            await RespondAsync(embed: new EmbedBuilder() {
                Title = EconomyLocale.EconomyEmbedTransfer_title,
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder() {
                        Name = EconomyLocale.EconomyEmbedTransfer_field0Name,
                        Value = user.Username
                    },
                    new EmbedFieldBuilder() {
                        Name = EconomyLocale.EconomyEmbedTransfer_field1Name,
                        Value = amount.ToString()
                    }
                },
                Color = Color.Green
            }.Build());
        }

        [SlashCommand("setbalance", "Sets the balance of a user.", runMode: RunMode.Async)]
        public async Task SetBalance(IUser user, int amount) {
            await economyRepository.AddEconomyAsync(Context.Guild.Id, user.Id, amount, 0);
            await economyRepository.SaveAsync();
            await RespondAsync(embed: new EmbedBuilder() {
                Title = EconomyLocale.EconomyEmbedSetBal_title,
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder() {
                        Name = EconomyLocale.EconomyEmbedSetBal_field0Name,
                        Value = user.Username
                    },
                    new EmbedFieldBuilder() {
                        Name = EconomyLocale.EconomyEmbedSetBal_field1Name,
                        Value = amount.ToString()
                    }
                },
                Color = Color.Green
            }.Build());
        }
    }
}
