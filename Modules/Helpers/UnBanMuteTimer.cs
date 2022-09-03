using Discord;
using Discord.WebSocket;
using Finder.Database.Repositories.Bot;
using System.Timers;

namespace Finder.Bot.Modules.Helpers;

public static class UnBanMuteTimer {
    private static System.Timers.Timer messageTimer;
    private static DiscordShardedClient client;
    private static UserLogsRepository userLogsRepository;
    private static SettingsRepository settingsRepository;
    public static void StartTimer(DiscordShardedClient _client, UserLogsRepository _userLogsRepository, SettingsRepository _settingsRepository) {
        client = _client;
        userLogsRepository = _userLogsRepository;
        settingsRepository = _settingsRepository;
        messageTimer = new System.Timers.Timer(5000);
        messageTimer.Elapsed += OnTimerElapsed;
        messageTimer.AutoReset = true;
        messageTimer.Enabled = true;
    }

    private static async void OnTimerElapsed(object source, ElapsedEventArgs e) {
        foreach (var c in (await userLogsRepository.GetAllAsync())) {
            var guild = client.GetGuild((ulong)c.GuildId);
            SocketGuildUser user;
            if (c.TempBan != null && c.TempBan < DateTime.UtcNow) {
                await guild.RemoveBanAsync((ulong)c.UserId);
                try {
                    user = guild.GetUser((ulong)c.UserId);
                    await user.SendMessageAsync(embed: new EmbedBuilder {
                        Title = "You have been unbanned",
                        Color = Color.Red,
                        Fields = new List<EmbedFieldBuilder> {
                            new EmbedFieldBuilder {
                                Name = "Server",
                                Value = $"{guild.Name}",
                                IsInline = false
                            }
                        },
                        Footer = new EmbedFooterBuilder {
                            Text = "FinderBot"
                        },
                        ThumbnailUrl = guild.IconUrl
                    }.Build());
                } catch (Exception) {
                    // ignored
                }
                await userLogsRepository.RemoveTempbanTime((ulong)c.GuildId, (ulong)c.UserId);
                await userLogsRepository.SaveAsync();
                continue;
            }
            if (c.TempMute == null || !(c.TempMute < DateTime.UtcNow)) continue;
            try {
                user = guild.GetUser((ulong)c.UserId);
                await user.SendMessageAsync(embed: new EmbedBuilder {
                    Title = "You have been Unmuted",
                    Color = Color.Red,
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = "Server",
                            Value = $"{guild.Name}",
                            IsInline = false
                        }
                    },
                    Footer = new EmbedFooterBuilder {
                        Text = "FinderBot"
                    },
                    ThumbnailUrl = guild.IconUrl
                }.Build());
                var muteRole = guild.GetRole(Convert.ToUInt64(await settingsRepository.GetSettingAsync(guild.Id, "muteRoleId")));
                await user.RemoveRoleAsync(muteRole);
            } catch (Exception) {
                // ignored
            }
            await userLogsRepository.RemoveTempmuteTime((ulong)c.GuildId, (ulong)c.UserId);
            await userLogsRepository.SaveAsync();
        }
    }
}