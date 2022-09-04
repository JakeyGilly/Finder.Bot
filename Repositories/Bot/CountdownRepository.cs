using Finder.Bot.Database;
using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;

public class CountdownRepository : Repository<CountdownModel> {
    public CountdownRepository(ApplicationContext context) : base(context) { }

    public async Task<CountdownModel> GetCountdownModelAsync(ulong messageId, ulong channelId, ulong guildId) {
        return await Context.Set<CountdownModel>().FindAsync((long)messageId, (long)channelId, (long)guildId) ?? new CountdownModel();
    }

    public async Task AddCountdownAsync(ulong messageId, ulong channelId, ulong guildId, DateTime dateTime, ulong? pingUserId = null, ulong? pingRoleId = null) {
        var addons = await Context.Set<CountdownModel>().FindAsync((long)messageId, (long)channelId, (long)guildId);
        if (addons == null) {
            await Context.Set<CountdownModel>().AddAsync(new CountdownModel {
                MessageId = (long)messageId,
                ChannelId = (long)channelId,
                GuildId = (long)guildId,
                DateTime = dateTime,
                PingUserId = (long?)pingUserId ?? null,
                PingRoleId = (long?)pingRoleId ?? null
            });
            return;
        }
        addons.MessageId = (long)messageId;
        addons.ChannelId = (long)channelId;
        addons.DateTime = dateTime;
        addons.PingUserId = (long?)pingUserId ?? null;
        addons.PingRoleId = (long?)pingRoleId ?? null;
        Context.Set<CountdownModel>().Update(addons);
    }

    public async Task RemoveCountdownAsync(ulong messageId, ulong channelId, ulong guildId) {
        var addons = await Context.Set<CountdownModel>().FindAsync((long)messageId, (long)channelId, (long)guildId);
        if (addons == null) return;
        Context.Set<CountdownModel>().Remove(addons);
    }
}