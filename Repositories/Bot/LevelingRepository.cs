using Finder.Bot.Database;
using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;
    
public class LevelingRepository : Repository<LevelingModel>, ILevelingRepository {
    public LevelingRepository(ApplicationContext context) : base(context) { }

    public async Task AddLevelingAsync(ulong guildId, ulong userId, int level, int exp) {
        var leveling = await FindAsync((long)guildId, (long)userId);
        if (leveling == null) {
            await AddAsync(new LevelingModel {
                GuildId = (long)guildId,
                UserId = (long)userId,
                Level = level,
                Exp = exp
            });
            return;
        }
        leveling.GuildId = (long)guildId;
        leveling.UserId = (long)userId;
        leveling.Level = level;
        leveling.Exp = exp;
        Update(leveling);
    }
}