using Finder.Bot.Database;
using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;

public class EconomyRepository : Repository<EconomyModel>, IEconomyRepository {
    public EconomyRepository(ApplicationContext context) : base(context) { }

    public async Task AddEconomyAsync(ulong guildId, ulong userId, int money, int bank) {
        var economy = await FindAsync((long)guildId, (long)userId);
        if (economy == null) {
            await AddAsync(new EconomyModel {
                GuildId = (long)guildId,
                UserId = (long)userId,
                Money = money,
                Bank = bank
            });
            return;
        }
        economy.GuildId = (long)guildId;
        economy.UserId = (long)userId;
        economy.Money += money;
        economy.Bank += bank;
        Update(economy);
    }

    public async Task SubtractEconomyAsync(ulong guildId, ulong userId, int money, int bank) {
        var economy = await FindAsync((long)guildId, (long)userId);
        if (economy == null) {
            await AddAsync(new EconomyModel {
                GuildId = (long)guildId,
                UserId = (long)userId,
                Money = money,
                Bank = bank
            });
            return;
        }
        economy.GuildId = (long)guildId;
        economy.UserId = (long)userId;
        economy.Money -= money;
        economy.Bank -= bank;
        Update(economy);
    }
}