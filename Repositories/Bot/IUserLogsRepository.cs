using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;
public interface IUserLogsRepository : IRepository<UserLogsModel> {
    Task AddUserLogsAsync(ulong guildId, ulong userId, int bans, int kicks, int warns, int mutes);
    Task AddTempbanTime(ulong guildId, ulong userId, DateTime time);
    Task RemoveTempbanTime(ulong guildId, ulong userId);
    Task AddTempmuteTime(ulong guildId, ulong userId, DateTime time);
    Task RemoveTempmuteTime(ulong guildId, ulong userId);
    Task<bool> UserLogsExistsAsync(ulong guildId, ulong userId);
    Task<bool> IsUnbanned(ulong guildId, ulong userId);
    Task<bool> IsUnmuted(ulong guildId, ulong userId);
    
}