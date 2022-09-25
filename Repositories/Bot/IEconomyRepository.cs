using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;
public interface IEconomyRepository : IRepository<EconomyModel> {
    Task AddEconomyAsync(ulong guildId, ulong userId, int money, int bank);
    Task SubtractEconomyAsync(ulong guildId, ulong userId, int money, int bank);
}