using Finder.Bot.Database;
using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;

public class ItemInvRepository : Repository<ItemInvModel> {
    public ItemInvRepository(ApplicationContext context) : base(context) { }
    
    public async Task<ItemInvModel?> GetItemsModelAsync(Int64 guildId, Int64 userId) {
        return await Context.Set<ItemInvModel>().FindAsync(guildId, userId);
    }
    
    public async Task AddItemAsync(ulong guildId, ulong userId, Guid itemId, int amount) {
        var items = await Context.Set<ItemInvModel>().FindAsync((long)guildId, (long)userId);
        if (items == null) {
            await Context.Set<ItemInvModel>().AddAsync(new ItemInvModel {
                GuildId = (long)guildId,
                UserId = (long)userId,
                ItemIds = new List<Guid>(),
            });
        }
        for (int i = 0; i < amount; i++) {
            items?.ItemIds.Add(itemId);
        }
        if (items != null) Context.Set<ItemInvModel>().Update(items);
    }
    
    public async Task RemoveItemAsync(ulong guildId, ulong userId, Guid itemId, int amount) {
        var items = await Context.Set<ItemInvModel>().FindAsync((long)guildId, (long)userId);
        if (items == null) return;
        if (!items.ItemIds.Contains(itemId)) {
            throw new Exception("Item not found in inventory");
        }
        if (items.ItemIds.Count(x => x == itemId) < amount) {
            throw new Exception("Not enough items in inventory");
        }
        for (int i = 0; i < amount; i++) {
            items.ItemIds.Remove(itemId);
        }
        Context.Set<ItemInvModel>().Update(items);
    }
    
    public async Task<bool> ItemExistsAsync(ulong guildId, ulong userId, Guid itemId) {
        var items = await Context.Set<ItemInvModel>().FindAsync((long)guildId, (long)userId);
        return items != null && items.ItemIds.Contains(itemId);
    }

    public async Task<bool> ItemsExistsAsync(ulong guildId, ulong userId) {
        var items = await Context.Set<ItemInvModel>().FindAsync((long)guildId, (long)userId);
        return items != null && items.ItemIds.Count > 0;
    }
}