using Finder.Bot.Database;
using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;

public class AddonsRepository : Repository<AddonsModel>, IAddonsRepository {
    public AddonsRepository(ApplicationContext context) : base(context) { }

    public async Task<string?> GetAddonAsync(ulong guildId, string key) {
        var addon = await GetAsync(guildId);
        return addon?.Addons[key];
    }
    
    public async Task AddAddonAsync(ulong guildId, string key, string value) {
        var addon = await GetAsync(guildId);
        if (addon == null) {
            await AddAsync(new AddonsModel {
                GuildId = (long)guildId,
                Addons = new Dictionary<string, string> {
                    { key, value }
                }
            });
            return;
        }
        if (addon.Addons.ContainsKey(key)) {
            addon.Addons[key] = value;
        } else {
            addon.Addons.Add(key, value);
        }
        Update(addon);
    }
    
    public async Task RemoveAddonAsync(ulong guildId, string key) {
        var addon = await GetAsync(guildId);
        if (addon == null) return;
        if (addon.Addons.ContainsKey(key) && addon.Addons[key] == "true") {
            addon.Addons[key] = null;
        }
        Update(addon);
    }

    public async Task<bool> AddonEnabled(ulong guildId, string key) {
        var addon = await GetAsync(guildId);
        return addon != null && addon.Addons.ContainsKey(key) && addon.Addons[key] == "true";
    }
}