using Finder.Bot.Database;
using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;

public class SettingsRepository : Repository<SettingsRepository> {
    public SettingsRepository(ApplicationContext context) : base(context) { }

    public async Task<SettingsModel> GetSettingModelsAsync(ulong guildId) {
        return await Context.Set<SettingsModel>().FindAsync((long)guildId) ?? new SettingsModel();
    }

    public async Task AddSettingAsync(ulong guildId, string key, string value) {
        var userSettings = await Context.Set<SettingsModel>().FindAsync((long)guildId);
        if (userSettings == null) {
            await Context.Set<SettingsModel>().AddAsync(new SettingsModel {
                GuildId = (long)guildId,
                Settings = new Dictionary<string, string> {
                    { key, value }
                }
            });
            return;
        }
        if (userSettings.Settings.ContainsKey(key)) {
            userSettings.Settings[key] = value;
        } else {
            userSettings.Settings.Add(key, value);
        }
        Context.Set<SettingsModel>().Update(userSettings);
    }

    public async Task<bool> SettingExists(ulong guildId, string key) {
        var settings = await Context.Set<SettingsModel>().FindAsync((long)guildId);
        return settings != null && settings.Settings.ContainsKey(key);
    }
    
    public async Task<string?> GetSettingAsync(ulong guildId, string key) {
        var settings = await Context.Set<SettingsModel>().FindAsync((long)guildId);
        return settings?.Settings.GetValueOrDefault(key);
    }
}