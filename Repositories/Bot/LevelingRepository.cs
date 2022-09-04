using Finder.Bot.Database;
using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;
    
public class LevelingRepository : Repository<LevelingModel> {
    public LevelingRepository(ApplicationContext context) : base(context) { }

    public async Task<LevelingModel> GetLevelingModelAsync(ulong guildId, ulong userId) {
        return await Context.Set<LevelingModel>().FindAsync((long)guildId, (long)userId) ?? new LevelingModel();
    }

    public async Task AddLevelingAsync(ulong guildId, ulong userId, int level, int exp) {
        var leveling = await Context.Set<LevelingModel>().FindAsync((long)guildId, (long)userId);
        if (leveling == null) {
            await Context.Set<LevelingModel>().AddAsync(new LevelingModel {
                GuildId = (long)guildId,
                UserId = (long)userId,
                Level = level,
                Exp = exp,
            });
            return;
        }
        leveling.GuildId = (long)guildId;
        leveling.UserId = (long)userId;
        leveling.Level = level;
        leveling.Exp = exp;
        Context.Set<LevelingModel>().Update(leveling);
    }

    public async Task RemoveLevelingAsync(ulong guildId, ulong userId) {
        var leveling = await Context.Set<LevelingModel>().FindAsync((long)guildId, (long)userId);
        if (leveling == null) return;
        Context.Set<LevelingModel>().Remove(leveling);
    }
}