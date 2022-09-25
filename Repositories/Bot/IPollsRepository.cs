using Finder.Bot.Models.Data.Bot;
namespace Finder.Bot.Repositories.Bot;
public interface IPollsRepository : IRepository<PollsModel> {
    Task AddPollsAsync(ulong messageId, List<string> answers, List<Int64> votersId);
    Task<bool> PollExistsAsync(ulong messageId);
}