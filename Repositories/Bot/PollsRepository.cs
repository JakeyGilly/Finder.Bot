using Finder.Bot.Database;
using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;

public class PollsRepository : Repository<PollsModel> {
    public PollsRepository(ApplicationContext context) : base(context) { }
    
    public async Task<PollsModel> GetPollsModelAsync(ulong messageId) {
        return await Context.Set<PollsModel>().FindAsync((long)messageId) ?? new PollsModel();
    }

    public async Task AddPollsAsync(ulong messageId, List<string> answers, List<Int64> votersId) {
        var polls = await Context.Set<PollsModel>().FindAsync((long)messageId);
        if (polls == null) {
            await Context.Set<PollsModel>().AddAsync(new PollsModel {
                MessageId = (long)messageId,
                Answers = answers,
                VotersId = votersId,
            });
            return;
        }
        polls.MessageId = (long)messageId;
        polls.Answers = answers;
        polls.VotersId = votersId;
        Context.Set<PollsModel>().Update(polls);
    }

    public async Task RemovePollsAsync(ulong messageId) {
        var polls = await Context.Set<PollsModel>().FindAsync((long)messageId);
        if (polls == null) return;
        Context.Set<PollsModel>().Remove(polls);
    }

    public async Task<bool> PollExistsAsync(ulong messageId) {
        return await Context.Set<PollsModel>().FindAsync((long)messageId) != null;
    }
}