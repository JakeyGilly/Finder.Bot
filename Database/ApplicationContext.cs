using Finder.Bot.Models.Data.Bot;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
namespace Finder.Bot.Database;

public class ApplicationContext : DbContext {
    public ApplicationContext(DbContextOptions options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder builder) {
        builder.Entity<UserLogsModel>().HasKey(table => new {
            table.GuildId, table.UserId
        });
        builder.Entity<CountdownModel>().HasKey(table => new {
            messageId = table.MessageId,
            channelId = table.ChannelId,
            guildId = table.GuildId
        });
        builder.Entity<LevelingModel>().HasKey(table => new {
            table.GuildId, table.UserId
        });
        builder.Entity<EconomyModel>().HasKey(table => new {
            guildId = table.GuildId,
            userId = table.UserId
        });
        builder.Entity<TicketsModel>().HasKey(table => new {
            table.GuildId, table.SupportChannelId
        });
        builder.Entity<ItemInvModel>().HasKey(table => new {
            guildId = table.GuildId,
            userId = table.UserId,
        });
        builder.Entity<AddonsModel>().Property(x => x.Addons).HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v) ?? new Dictionary<string, string>());
        builder.Entity<SettingsModel>().Property(x => x.Settings).HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v) ?? new Dictionary<string, string>());
    }
    
    // Finder.Bot
    public DbSet<AddonsModel> Addons { get; set; }
    public DbSet<UserLogsModel> UserLogs { get; set; }
    public DbSet<SettingsModel> Settings { get; set; }
    public DbSet<PollsModel> Polls { get; set; }
    public DbSet<CountdownModel> Countdowns { get; set; }
    public DbSet<TicketsModel> Tickets { get; set; }
    public DbSet<LevelingModel> Leveling { get; set; }
    public DbSet<EconomyModel> Economy { get; set; }
    public DbSet<ItemInvModel> ItemInventory { get; set; }
}
