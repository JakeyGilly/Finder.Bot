using Finder.Bot.Database;
using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;

public class EconomyRepository : Repository<EconomyModel> {
    public EconomyRepository(ApplicationContext context) : base(context) { }
    
    public async Task<EconomyModel> GetEconomyModelAsync(ulong guildId, ulong userId) {
        return await Context.Set<EconomyModel>().FindAsync((long)guildId, (long)userId) ?? new EconomyModel();
    }

    public async Task AddEconomyAsync(ulong guildId, ulong userId, int money, int bank) {
        var economy = await Context.Set<EconomyModel>().FindAsync((long)guildId, (long)userId);
        if (economy == null) {
            await Context.Set<EconomyModel>().AddAsync(new EconomyModel {
                GuildId = (long)guildId,
                UserId = (long)userId,
                Money = money,
                Bank = bank
            });
            return;
        }
        economy.GuildId = (long)guildId;
        economy.UserId = (long)userId;
        economy.Money = economy.Money + money;
        economy.Bank = economy.Bank + bank;
        Context.Set<EconomyModel>().Update(economy);
    }

    public async Task SubtractEconomyAsync(ulong guildId, ulong userId, int money, int bank) {
        var economy = await Context.Set<EconomyModel>().FindAsync((long)guildId, (long)userId);
        if (economy == null) {
            await Context.Set<EconomyModel>().AddAsync(new EconomyModel {
                GuildId = (long)guildId,
                UserId = (long)userId,
                Money = money,
                Bank = bank
            });
            return;
        }
        economy.GuildId = (long)guildId;
        economy.UserId = (long)userId;
        economy.Money = economy.Money - money;
        economy.Bank = economy.Bank - bank;
        Context.Set<EconomyModel>().Update(economy);
    }

    public async Task RemoveEconomyAsync(ulong guildId, ulong userId) {
        var economy = await Context.Set<EconomyModel>().FindAsync((long)guildId, (long)userId);
        if (economy == null) return;
        Context.Set<EconomyModel>().Remove(economy);
    }
}