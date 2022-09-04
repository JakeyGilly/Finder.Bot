using Finder.Bot.Database;
using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;

public class AddonsRepository : Repository<AddonsRepository> {
    public AddonsRepository(ApplicationContext context) : base(context) { }

    public async Task<AddonsModel?> GetAddonsModelAsync(ulong guildId) {
        return await Context.Set<AddonsModel>().FindAsync((long)guildId);
    }

    public async Task AddAddonAsync(ulong guildId, string key, string value) {
        var addon = await Context.Set<AddonsModel>().FindAsync((long)guildId);
        if (addon == null) {
            await Context.Set<AddonsModel>().AddAsync(new AddonsModel {
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
        Context.Set<AddonsModel>().Update(addon);
    }
    
    public async Task RemoveAddonAsync(ulong guildId, string key) {
        var addon = await Context.Set<AddonsModel>().FindAsync((long)guildId);
        if (addon == null) return;
        if (addon.Addons.ContainsKey(key) && addon.Addons[key] == "true") addon.Addons[key] = null;
        Context.Set<AddonsModel>().Update(addon);
    }

    public async Task<bool> AddonEnabled(ulong guildId, string key) {
        var addon = await Context.Set<AddonsModel>().FindAsync((long)guildId);
        if (addon == null) return false;
        return addon.Addons.ContainsKey(key) && addon.Addons[key] == "true";
    }
    
    public async Task<string?> GetAddonAsync(ulong guildId, string key) {
        var addon = await Context.Set<AddonsModel>().FindAsync((long)guildId);
        return addon?.Addons.GetValueOrDefault(key);
    }
}