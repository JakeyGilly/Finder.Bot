using Finder.Bot.Database;
using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;
    
public class UserLogsRepository : Repository<UserLogsModel>, IUserLogsRepository {
    public UserLogsRepository(ApplicationContext context) : base(context) { }

    public async Task AddUserLogsAsync(ulong guildId, ulong userId, int bans, int kicks, int warns, int mutes) {
        var userLogs = await FindAsync((long)guildId, (long)userId);
        if (userLogs == null) {
            await AddAsync(new UserLogsModel {
                GuildId = (long)guildId,
                UserId = (long)userId,
                Bans = bans,
                Kicks = kicks,
                Warns = warns,
                Mutes = mutes
            });
            return;
        }
        userLogs.GuildId = (long)guildId;
        userLogs.UserId = (long)userId;
        userLogs.Bans = bans;
        userLogs.Kicks = kicks;
        userLogs.Warns = warns;
        userLogs.Mutes = mutes;
        Update(userLogs);
    }
    
    public async Task AddTempbanTime(ulong guildId, ulong userId, DateTime time) {
        var userLogs = await FindAsync((long)guildId, (long)userId);
        if (userLogs == null) {
            await AddAsync(new UserLogsModel {
                GuildId = (long)guildId,
                UserId = (long)userId,
                TempBan = time
            });
            return;
        }
        userLogs.TempBan = time;
        Update(userLogs);
    }
    
    public async Task RemoveTempbanTime(ulong guildId, ulong userId) {
        var userLogs = await FindAsync((long)guildId, (long)userId);
        if (userLogs == null) return;
        userLogs.TempBan = null;
        Update(userLogs);
    }
    
    public async Task AddTempmuteTime(ulong guildId, ulong userId, DateTime time) {
        var userLogs = await FindAsync((long)guildId, (long)userId);
        if (userLogs == null) {
            await AddAsync(new UserLogsModel {
                GuildId = (long)guildId,
                UserId = (long)userId,
                TempMute = time
            });
            return;
        }
        userLogs.TempMute = time;
        Update(userLogs);
    }
    
    public async Task RemoveTempmuteTime(ulong guildId, ulong userId) {
        var userLogs = await FindAsync((long)guildId, (long)userId);
        if (userLogs == null) return;
        userLogs.TempMute = null;
        Update(userLogs);
    }

    public async Task<bool> UserLogsExistsAsync(ulong guildId, ulong userId) {
        return await FindAsync((long)guildId, (long)userId) != null;
    }
    
    public async Task<bool> IsUnbanned(ulong guildId, ulong userId) {
        var userLogs = await FindAsync((long)guildId, (long)userId);
        if (userLogs == null) return false;
        return userLogs.TempBan > DateTime.UtcNow;
    }
    
    public async Task<bool> IsUnmuted(ulong guildId, ulong userId) {
        var userLogs = await FindAsync((long)guildId, (long)userId);
        if (userLogs == null) return false;
        return userLogs.TempMute > DateTime.UtcNow;
    }
}