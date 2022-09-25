using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;
public interface ILevelingRepository : IRepository<LevelingModel> {
    Task AddLevelingAsync(ulong guildId, ulong userId, int level, int exp);
}