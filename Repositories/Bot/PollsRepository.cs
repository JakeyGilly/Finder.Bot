using Finder.Bot.Database;
using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;

public class PollsRepository : Repository<PollsModel>, IPollsRepository {
    public PollsRepository(ApplicationContext context) : base(context) { }

    public async Task AddPollsAsync(ulong messageId, List<string> answers, List<Int64> votersId) {
        var polls = await GetAsync(messageId);
        if (polls == null) {
            await AddAsync(new PollsModel {
                MessageId = (long)messageId,
                Answers = answers,
                VotersId = votersId,
            });
            return;
        }
        polls.MessageId = (long)messageId;
        polls.Answers = answers;
        polls.VotersId = votersId;
        Update(polls);
    }

    public async Task<bool> PollExistsAsync(ulong messageId) {
        var polls = await GetAsync(messageId);
        return polls != null;
    }
}