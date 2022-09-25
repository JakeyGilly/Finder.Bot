using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;
public interface ICountdownRepository : IRepository<CountdownModel> {
    Task AddCountdownAsync(ulong messageId, ulong channelId, ulong guildId, DateTime dateTime, ulong? pingUserId = null, ulong? pingRoleId = null);
}