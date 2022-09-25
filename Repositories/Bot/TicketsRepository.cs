using Finder.Bot.Database;
using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;
    
public class TicketsRepository : Repository<TicketsModel>, ITicketsRepository {
    public TicketsRepository(ApplicationContext context) : base(context) { }

    public async Task AddTicketAsync(ulong guildId, ulong supportChannelId, ulong? introMessageId, List<Int64?> userIds, string? name, List<Int64> claimedUserIds) {
        var tickets = await GetAsync(supportChannelId);
        if (tickets == null) {
            await AddAsync(new TicketsModel {
                GuildId = (long)guildId,
                SupportChannelId = (long)supportChannelId,
                IntroMessageId = (long?)introMessageId ?? null,
                Name = name,
                ClaimedUserId = claimedUserIds,
                UserIds = userIds
            });
            return;
        }
        tickets.GuildId = (long)guildId;
        tickets.SupportChannelId = (long)supportChannelId;
        tickets.IntroMessageId = (long)introMessageId!;
        tickets.UserIds = userIds;
        tickets.Name = name;
        tickets.ClaimedUserId = claimedUserIds;
        Update(tickets);
    }

    public async Task AddTicketUserIdAsync(ulong supportChannelId, ulong userId) {
        var tickets = await GetAsync(supportChannelId);
        tickets?.UserIds.Add((long)userId);
        Update(tickets ?? throw new InvalidOperationException());
    }
    
    public async Task RemoveTicketUserIdAsync(ulong supportChannelId, ulong userId) {
        var tickets = await GetAsync(supportChannelId);
        tickets?.UserIds.Remove((long)userId);
        Update(tickets ?? throw new InvalidOperationException());
    }
    
    public async Task AddTicketClaimedUserIdAsync(ulong supportChannelId, ulong userId) {
        var tickets = await GetAsync(supportChannelId);
        tickets?.ClaimedUserId.Add((long)userId);
        Update(tickets ?? throw new InvalidOperationException());
    }
    
    public async Task RemoveTicketClaimedUserIdAsync(ulong supportChannelId, ulong userId) {
        var tickets = await GetAsync(supportChannelId);
        tickets?.ClaimedUserId.Remove((long)userId);
        Update(tickets ?? throw new InvalidOperationException());
    }
}