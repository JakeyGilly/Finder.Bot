using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;
public interface ISettingsRepository : IRepository<SettingsModel> {
    Task<string?> GetSettingAsync(ulong guildId, string key);
    Task AddSettingAsync(ulong guildId, string key, string value);
    Task<bool> SettingExists(ulong guildId, string key);
}