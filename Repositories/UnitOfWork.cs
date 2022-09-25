using Finder.Bot.Database;
using Finder.Bot.Repositories.Bot;
namespace Finder.Bot.Repositories;

public class UnitOfWork : IUnitOfWork {
    private readonly ApplicationContext _context;
    public IAddonsRepository Addons { get; }
    public ICountdownRepository Countdown { get; }
    public IEconomyRepository Economy { get; }
    public IItemInvRepository ItemInv { get; }
    public ILevelingRepository Leveling { get; }
    public IPollsRepository Polls { get; }
    public ISettingsRepository Settings { get; }
    public ITicketsRepository Tickets { get; }
    public IUserLogsRepository UserLogs { get; }
    public UnitOfWork(ApplicationContext context) {
        _context = context;
        Addons = new AddonsRepository(_context);
        Countdown = new CountdownRepository(_context);
        Economy = new EconomyRepository(_context);
        ItemInv = new ItemInvRepository(_context);
        Leveling = new LevelingRepository(_context);
        Polls = new PollsRepository(_context);
        Settings = new SettingsRepository(_context);
        Tickets = new TicketsRepository(_context);
        UserLogs = new UserLogsRepository(_context);
    }
    public async Task<int> SaveChangesAsync() {
        return await _context.SaveChangesAsync();
    }
    public async Task DisposeAsync() {
        await _context.DisposeAsync();
    }
}