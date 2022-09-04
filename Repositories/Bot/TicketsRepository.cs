using Finder.Bot.Database;
using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;
    
public class TicketsRepository : Repository<TicketsModel> {
    public TicketsRepository(ApplicationContext context) : base(context) { }
    public async Task<TicketsModel> GetTicketsModelAsync(ulong guildId, ulong supportChannelId) {
        return await Context.Set<TicketsModel>().FindAsync((long)guildId, (long)supportChannelId) ?? new TicketsModel();
    }

    public async Task AddTicketAsync(ulong guildId, ulong supportChannelId, ulong? introMessageId, List<Int64?> userIds, string? name, List<Int64> claimedUserIds) {
        var tickets = await Context.Set<TicketsModel>().FindAsync((long)guildId, (long)supportChannelId);
        if (tickets == null) {
            await Context.Set<TicketsModel>().AddAsync(new TicketsModel {
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
        tickets.IntroMessageId = ((long)introMessageId!);
        tickets.UserIds = userIds;
        tickets.Name = name;
        tickets.ClaimedUserId = claimedUserIds;
    }

    public async Task RemoveTicketAsync(ulong guildId, ulong supportChannelId) {
        var tickets = await Context.Set<TicketsModel>().FindAsync((long)guildId, (long)supportChannelId);
        if (tickets == null) return;
        Context.Set<TicketsModel>().Remove(tickets);
    }
    
    public async Task AddTicketUserIdAsync(ulong guildId, ulong supportChannelId, ulong userId) {
        var tickets = await Context.Set<TicketsModel>().FindAsync((long)guildId, (long)supportChannelId);
        tickets?.UserIds.Add((long)userId);
    }
    
    public async Task RemoveTicketUserIdAsync(ulong guildId, ulong supportChannelId, ulong userId) {
        var tickets = await Context.Set<TicketsModel>().FindAsync((long)guildId, (long)supportChannelId);
        tickets?.UserIds.Remove((long)userId);
    }
    
    public async Task AddTicketClaimedUserIdAsync(ulong guildId, ulong supportChannelId, ulong userId) {
        var tickets = await Context.Set<TicketsModel>().FindAsync((long)guildId, (long)supportChannelId);
        tickets?.ClaimedUserId.Add((long)userId);
    }
    
    public async Task RemoveTicketClaimedUserIdAsync(ulong guildId, ulong supportChannelId, ulong userId) {
        var tickets = await Context.Set<TicketsModel>().FindAsync((long)guildId, (long)supportChannelId);
        tickets?.ClaimedUserId.Remove((long)userId);
    }
}