using Discord;
using Discord.WebSocket;
using Finder.Bot.Repositories.Bot;
using System.Timers;

namespace Finder.Bot.Modules.Helpers;

public static class CountdownTimer {
    private static System.Timers.Timer messageTimer;
    private static DiscordShardedClient client;
    private static CountdownRepository countdownRepository;
    public static void StartTimer(DiscordShardedClient _client, CountdownRepository _countdownRepository) {
        client = _client;
        countdownRepository = _countdownRepository;
        messageTimer = new System.Timers.Timer(3000);
        messageTimer.Elapsed += OnTimerElapsed;
        messageTimer.AutoReset = true;
        messageTimer.Enabled = true;
    }

    public static async void OnTimerElapsed(object source, ElapsedEventArgs e) {
        foreach (var c in (await countdownRepository.GetAllAsync())) {
            var guild = client.GetGuild((ulong)c.GuildId);
            var channel = (ITextChannel)guild.GetChannel((ulong)c.ChannelId);
            var messages = (IUserMessage) await channel.GetMessageAsync((ulong)c.MessageId);
            if (c.DateTime < DateTime.UtcNow || messages == null) {
                if (messages != null) {
                    await messages.ModifyAsync(x => x.Embed = new EmbedBuilder {
                        Title = "Countdown",
                        Fields = new List<EmbedFieldBuilder> {
                            new EmbedFieldBuilder {
                                Name = "Time left",
                                Value = "Countdown has ended"
                            }
                        },
                        Footer = new EmbedFooterBuilder {
                            Text = "FinderBot"
                        }
                    }.Build());
                    var userId = c.PingUserId;
                    var roleId = c.PingRoleId;
                    if (userId != null || roleId != null) {
                        SocketGuildUser? user = null;
                        SocketRole? role = null;
                        try {
                            user = guild.GetUser((ulong)userId);
                        } catch (Exception) {
                            // ignored
                        }
                        try {
                            role = guild.GetRole((ulong)roleId);
                        } catch (Exception) {
                            // ignored
                        }
                        if (user != null || role != null) {
                            await channel.SendMessageAsync(user?.Mention ?? role?.Mention);
                        }
                    }
                }
                await countdownRepository.RemoveCountdownAsync((ulong)c.MessageId, (ulong)c.ChannelId, (ulong)c.GuildId);
                await countdownRepository.SaveAsync();
                continue;
            }
            var timeLeft = c.DateTime - DateTime.Now.ToUniversalTime();
            await messages.ModifyAsync(x => x.Embed = new EmbedBuilder {
                Title = "Countdown",
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder {
                        Name = "Time left",
                        Value = CountdownModule.HumanizeTime(timeLeft)
                    }
                },
                Footer = new EmbedFooterBuilder {
                    Text = "FinderBot"
                }
            }.Build());
        }
    }
}