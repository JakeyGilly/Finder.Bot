using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;
public interface IItemInvRepository : IRepository<ItemInvModel> {
    Task AddItemAsync(ulong guildId, ulong userId, Guid itemId, int amount);
    Task RemoveItemAsync(ulong guildId, ulong userId, Guid itemId, int amount);
    Task<bool> ItemExistsAsync(ulong guildId, ulong userId, Guid itemId);
    Task<bool> ItemsExistsAsync(ulong guildId, ulong userId);
}