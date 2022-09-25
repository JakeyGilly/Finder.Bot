using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories; 

public interface ITicketsRepository : IRepository<TicketsModel> {
    Task AddTicketAsync(ulong guildId, ulong supportChannelId, ulong? introMessageId, List<Int64?> userIds, string? name, List<Int64> claimedUserIds);
    Task AddTicketUserIdAsync(ulong supportChannelId, ulong userId);
    Task RemoveTicketUserIdAsync(ulong supportChannelId, ulong userId);
    Task AddTicketClaimedUserIdAsync(ulong supportChannelId, ulong userId);
    Task RemoveTicketClaimedUserIdAsync(ulong supportChannelId, ulong userId);
}