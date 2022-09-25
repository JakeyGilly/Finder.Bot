using Finder.Bot.Repositories.Bot;
namespace Finder.Bot.Repositories;

public interface IUnitOfWork {
    IAddonsRepository Addons { get; }
    ICountdownRepository Countdown { get; }
    IEconomyRepository Economy { get; }
    IItemInvRepository ItemInv { get; }
    ILevelingRepository Leveling { get; }
    IPollsRepository Polls { get; }
    ISettingsRepository Settings { get; }
    ITicketsRepository Tickets { get; }
    IUserLogsRepository UserLogs { get; }
    Task<int> SaveChangesAsync();
    Task DisposeAsync();
}