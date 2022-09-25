using Finder.Bot.Database;
using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;

public class ItemInvRepository : Repository<ItemInvModel>, IItemInvRepository {
    public ItemInvRepository(ApplicationContext context) : base(context) { }

    public async Task AddItemAsync(ulong guildId, ulong userId, Guid itemId, int amount) {
        var items = await FindAsync((long)guildId, (long)userId);
        if (items == null) {
            await AddAsync(new ItemInvModel {
                GuildId = (long)guildId,
                UserId = (long)userId,
                ItemIds = new List<Guid>(),
            });
        }
        for (int i = 0; i < amount; i++) items?.ItemIds.Add(itemId);
        if (items == null) return;
        Update(items);
    }
    
    public async Task RemoveItemAsync(ulong guildId, ulong userId, Guid itemId, int amount) {
        var items = await FindAsync((long)guildId, (long)userId);
        if (items == null) return;
        if (!items.ItemIds.Contains(itemId)) {
            throw new Exception("Item not found in inventory");
        }
        if (items.ItemIds.Count(x => x == itemId) < amount) {
            throw new Exception("Not enough items in inventory");
        }
        for (int i = 0; i < amount; i++) items.ItemIds.Remove(itemId);
        Update(items);
    }
    
    public async Task<bool> ItemExistsAsync(ulong guildId, ulong userId, Guid itemId) {
        var items = await FindAsync((long)guildId, (long)userId);
        return items?.ItemIds.Contains(itemId) ?? false;
    }

    public async Task<bool> ItemsExistsAsync(ulong guildId, ulong userId) {
        var items = await FindAsync((long)guildId, (long)userId);
        return items != null && items.ItemIds.Count > 0;
    }
}