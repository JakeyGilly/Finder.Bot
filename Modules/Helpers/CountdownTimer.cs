using Discord;
using Discord.WebSocket;
using Finder.Bot.Repositories;
using System.Timers;

namespace Finder.Bot.Modules.Helpers;

public static class CountdownTimer {
    private static DiscordShardedClient _client;
    private static System.Timers.Timer _messageTimer;
    private static IUnitOfWork _unitOfWork;
    public static void StartTimer(DiscordShardedClient client, IUnitOfWork unitOfWork) {
        _client = client;
        _unitOfWork = unitOfWork;
        _messageTimer = new System.Timers.Timer(3000);
        _messageTimer.Elapsed += OnTimerElapsed;
        _messageTimer.AutoReset = true;
        _messageTimer.Enabled = true;
    }

    private static async void OnTimerElapsed(object source, ElapsedEventArgs e) {
        foreach (var c in await _unitOfWork.Countdown.GetAllAsync()) {
            var guild = _client.GetGuild((ulong)c.GuildId);
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
                _unitOfWork.Countdown.Remove(c);
                await _unitOfWork.SaveChangesAsync();
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