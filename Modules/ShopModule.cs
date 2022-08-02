using Discord;
using Discord.Interactions;
using Finder.Database.Repositories;
using Finder.Database.Repositories.Bot;
using Finder.Shared;
using Finder.Shared.Models;
using Newtonsoft.Json;

namespace Finder.Bot.Modules {
    [Group("shop", "The shop commands to buy items.")]
    public class ShopModule : InteractionModuleBase<ShardedInteractionContext> {
        public static ItemInvRepository itemsRepository;
        public static EconomyRepository economyRepository;
        public ShopModule(ItemInvRepository _itemsRepository, EconomyRepository _economyRepository) {
            itemsRepository = _itemsRepository;
            economyRepository = _economyRepository;
        }
        public static ItemsRoot? itemsroot = JsonConvert.DeserializeObject<ItemsRoot>(File.ReadAllText(@"items.json"));
        [SlashCommand("buy", "Buy an item from the shop.")]
        public async Task BuyCommand([Autocomplete(typeof(ShopAutocompleteHandler))] string item, int amount = 1) {
            Guid itemId = Guid.Parse(item);
            if (itemsroot == null) {
                await ReplyAsync("Could not load items.");
                return;
            }
            var itemToBuy = itemsroot.Items.Find(x => x.Id == itemId);
            if (!itemToBuy.Buyable) {
                await RespondAsync("This item is not buyable.");
                return;
            }
            if ((await economyRepository.GetEconomyAsync(Context.Guild.Id, Context.User.Id)).Money < (itemToBuy.BuyPrice * amount)) {
                await RespondAsync("You do not have enough money to buy this item.");
                return;
            }
            await economyRepository.AddEconomyAsync(Context.Guild.Id, Context.User.Id, -(itemToBuy.BuyPrice * amount), 0);
            await itemsRepository.AddItemAsync(Context.Guild.Id, Context.User.Id, itemId, amount);
            await economyRepository.SaveAsync();
            await itemsRepository.SaveAsync();
            string amountstr = amount == 0 ? amount.ToString() : "an";
            await RespondAsync(embed: new EmbedBuilder() {
                Title = $"You have purchased {amountstr} item!",
                Fields = new List<EmbedFieldBuilder>() {
                    new EmbedFieldBuilder() {
                        Name = itemToBuy.Name,
                        Value = "For " + itemToBuy.BuyPrice * amount,
                    }
                },
                Color = Color.Green
            }.Build());
        }
        
        [SlashCommand("sell", "Sell an item to the shop.")]
        public async Task SellCommand([Autocomplete(typeof(InvAutocompleteHandler))] string item, int amount = 1) {
            Guid itemId = Guid.Parse(item);
            if (itemsroot == null) {
                await ReplyAsync("Could not load items.");
                return;
            }
            var itemToSell = itemsroot.Items.Find(x => x.Id == itemId);
            if (!itemToSell.Sellable) {
                await RespondAsync("This item is not sellable.");
                return;
            }
            if (!await itemsRepository.ItemExistsAsync(Context.Guild.Id, Context.User.Id, itemId)) {
                await RespondAsync("You do not have this item.");
                return;
            }
            await economyRepository.AddEconomyAsync(Context.Guild.Id, Context.User.Id, (itemToSell.SellPrice * amount), 0);
            await itemsRepository.RemoveItemAsync(Context.Guild.Id, Context.User.Id, itemId, amount);
            await economyRepository.SaveAsync();
            await itemsRepository.SaveAsync();
            string amountstr = amount == 0 ? amount.ToString() : "an";
            await RespondAsync(embed: new EmbedBuilder() {
                Title = $"You have sold {amountstr} item!",
                Fields = new List<EmbedFieldBuilder>() {
                    new EmbedFieldBuilder() {
                        Name = itemToSell.Name,
                        Value = "For " + itemToSell.SellPrice * amount,
                    }
                },
                Color = Color.Green
            }.Build());
        }

        [SlashCommand("info", "Displays item info in the shop")]
        public async Task InfoCommand([Autocomplete(typeof(ShopAutocompleteHandler))] string itemStr) {
            Guid itemId = Guid.Parse(itemStr);
            if (itemsroot == null) {
                await ReplyAsync("Could not load items.");
                return;
            }
            var item = itemsroot.Items.Find(x => x.Id == itemId);
            await RespondAsync(embed: new EmbedBuilder() {
                Title = $"{item.Name} infomation",
                Fields = new List<EmbedFieldBuilder>() {
                    new EmbedFieldBuilder() {
                        Name = "Description",
                        Value = item.Description,
                        IsInline = false
                    },
                    new EmbedFieldBuilder() {
                        Name = "Rarity",
                        Value = item.Rarity.ToString(),
                        IsInline = false
                    },
                    new EmbedFieldBuilder() {
                        Name = "Buy Price",
                        Value = item.Buyable ? item.BuyPrice : "Unbuyable",
                        IsInline = true
                    },
                    new EmbedFieldBuilder() {
                        Name = "Sell Price",
                        Value = item.Sellable ? item.SellPrice : "Unsellable",
                        IsInline = true
                    },
                    new EmbedFieldBuilder() {
                        Name = "Tradeable",
                        Value = item.Tradeable ? "Yes" : "No",
                        IsInline = true
                    },
                }
            }.Build());
        }
    }

    public class InventoryModule : InteractionModuleBase<ShardedInteractionContext> {
        public static ItemInvRepository itemsRepository;
        public InventoryModule(ItemInvRepository _itemsRepository) {
            itemsRepository = _itemsRepository;
        }
        public static ItemsRoot? itemsroot = JsonConvert.DeserializeObject<ItemsRoot>(File.ReadAllText(@"items.json"));

        [SlashCommand("inventory", "View your inventory.")]
        public async Task InventoryCommand() {
            var items = await itemsRepository.GetItemsAsync((long)Context.Guild.Id, (long)Context.User.Id);
            if (items == null || items.ItemIds.Count == 0) {
                await RespondAsync("You do not have any items.");
                return;
            }
            var embed = new EmbedBuilder() {
                Title = "Your inventory",
                Color = Color.Green
            };
            foreach (var item in items.ItemIds) {
                var itemToBuy = itemsroot.Items.Find(x => x.Id == item);
                var amount = items.ItemIds.Count(x => x == item);
                if (embed.Fields.Find(x => x.Name.Substring(0, itemToBuy.Name.Length) == itemToBuy.Name) == null) {
                    embed.AddField(itemToBuy.Name + " x" + amount, itemToBuy.Description);
                }
            }
            await RespondAsync(embed: embed.Build());
        }
    }
    public class ShopAutocompleteHandler : AutocompleteHandler {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {
            IEnumerable<AutocompleteResult> results = new List<AutocompleteResult>();
            if (ShopModule.itemsroot == null) {
                return AutocompletionResult.FromError(InteractionCommandError.Unsuccessful, "Could not load items.");
            }
            results = ShopModule.itemsroot.Items.Aggregate(results, (current, i) => current.Append(new AutocompleteResult(i.Name, i.Id.ToString())));
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }
    
    public class InvAutocompleteHandler : AutocompleteHandler {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {
            IEnumerable<AutocompleteResult> results = new List<AutocompleteResult>();
            var items = await ShopModule.itemsRepository.GetItemsAsync((long)context.Guild.Id, (long)context.User.Id);
            if (items == null || items.ItemIds.Count == 0) {
                return AutocompletionResult.FromError(InteractionCommandError.Unsuccessful, "You do not have any items.");
            }
            if (ShopModule.itemsroot == null) {
                return AutocompletionResult.FromError(InteractionCommandError.Unsuccessful, "Could not load items.");
            }
            foreach (var itemId in items.ItemIds) {
                var item = ShopModule.itemsroot.Items.Find(x => x.Id == itemId);
                if (item == null) continue;
                // if item is already in the list, skip it
                if (results.Any(x => x.Value.Equals(item.Id.ToString()))) continue;
                results = results.Append(new AutocompleteResult(item.Name, item.Id.ToString()));
            }
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }
    
}
