using Finder.Bot.Database;
using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;

public class CountdownRepository : Repository<CountdownModel>, ICountdownRepository {
    public CountdownRepository(ApplicationContext context) : base(context) { }

    public async Task AddCountdownAsync(ulong messageId, ulong channelId, ulong guildId, DateTime dateTime, ulong? pingUserId = null, ulong? pingRoleId = null) {
        var countdown = await GetAsync(messageId);
        if (countdown == null) {
            await AddAsync(new CountdownModel {
                MessageId = (long)messageId,
                ChannelId = (long)channelId,
                GuildId = (long)guildId,
                DateTime = dateTime,
                PingUserId = (long?)pingUserId ?? null,
                PingRoleId = (long?)pingRoleId ?? null
            });
            return;
        }
        countdown.MessageId = (long)messageId;
        countdown.ChannelId = (long)channelId;
        countdown.GuildId = (long)guildId;
        countdown.DateTime = dateTime;
        countdown.PingUserId = (long?)pingUserId ?? null;
        countdown.PingRoleId = (long?)pingRoleId ?? null;
        Update(countdown);
    }
}