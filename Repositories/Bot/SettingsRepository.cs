using Finder.Bot.Database;
using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;

public class SettingsRepository : Repository<SettingsModel>, ISettingsRepository {
    public SettingsRepository(ApplicationContext context) : base(context) { }
    
    public async Task<string?> GetSettingAsync(ulong guildId, string key) {
        var settings = await GetAsync(guildId);
        return settings?.Settings[key];
    }

    public async Task AddSettingAsync(ulong guildId, string key, string value) {
        var userSettings = await GetAsync(guildId);
        if (userSettings == null) {
            await AddAsync(new SettingsModel {
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
        Update(userSettings);
    }

    public async Task<bool> SettingExists(ulong guildId, string key) {
        var settings = await GetAsync(guildId);
        return settings != null && settings.Settings.ContainsKey(key);
    }
}